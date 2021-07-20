using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using ConsoleTables;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using SUs.KeepLatest.Cli.DataAccess;
using SUs.KeepLatest.Cli.DataAccess.Models;
using SUs.KeepLatest.Cli.Extensions;

namespace SUs.KeepLatest.Cli
{
    public static class RootCommandBuilder
    {
        public static RootCommand Build()
        {
            var root = new RootCommand();

            RegisterSubCommand(root);

            return root;
        }

        private static void RegisterSubCommand(RootCommand rootCommand)
        {
            RegisterCommandAdd(rootCommand);
            RegisterCommandUpdate(rootCommand);
            RegisterCommandRemove(rootCommand);
            RegisterCommandList(rootCommand);
            RegisterCommandFetch(rootCommand);
            RegisterCommandPull(rootCommand);
        }

        private static void RegisterCommandAdd(Command command)
        {
            var commandAdd = new Command("add", "Add an item");

            var optionName = new Option<string>("-name", "Display name or project name") { IsRequired = true };
            commandAdd.AddOption(optionName);

            var optionRepository = new Option<string>("-url", "Repository url") { IsRequired = true };
            commandAdd.AddOption(optionRepository);

            var optionVersion = new Option<string>("-version", "Current version");
            commandAdd.AddOption(optionVersion);

            commandAdd.Handler = CommandHandler.Create<string, string, string>(async (name, url, version) =>
            {
                using var db = new ApplicationDbContext();
                var isExists = await db.Items.AnyAsync(x => x.ItemName == name);
                if (isExists)
                {
                    Console.WriteLine($"{name} is already exists.");
                }
                else
                {
                    db.Items.Add(new KeepLatestItem
                    {
                        ItemName = name,
                        ItemUrl = url,
                        ItemVer = version
                    });

                    _ = await db.SaveChangesAsync();

                    Console.WriteLine($"{name} saved.");
                }
            });

            command.AddCommand(commandAdd);
        }

        private static void RegisterCommandUpdate(Command command)
        {
            var commandUpdate = new Command("update", "Update version");

            var optionName = new Option<string>("-name", "Display name or project name") { IsRequired = true };
            commandUpdate.AddOption(optionName);

            var optionRepository = new Option<string>("-url", "Repository url");
            commandUpdate.AddOption(optionRepository);

            var optionVersion = new Option<string>("-version", "Current version");
            commandUpdate.AddOption(optionVersion);

            commandUpdate.Handler = CommandHandler.Create<string, string, string>(async (name, url, version) =>
            {
                using var db = new ApplicationDbContext();
                var findItem = await db.Items.FirstOrDefaultAsync(x => x.ItemName == name);

                if (findItem == null)
                {
                    Console.WriteLine($"{name} not exists");
                    return;
                }

                if (!string.IsNullOrEmpty(url))
                {
                    findItem.ItemUrl = url;
                }

                if (!string.IsNullOrEmpty(version))
                {
                    findItem.ItemVer = version;
                }

                _ = await db.SaveChangesAsync();


                Console.WriteLine($"{name} updated.");
            });

            command.AddCommand(commandUpdate);
        }

        private static void RegisterCommandRemove(Command command)
        {
            var commandRemove = new Command("remove", "Delete an item");

            var optionName = new Option<string>("-name", "Display name or project name") { IsRequired = true };
            commandRemove.AddOption(optionName);

            commandRemove.Handler = CommandHandler.Create<string>(async name =>
            {
                using var db = new ApplicationDbContext();
                var findItem = await db.Items.FirstOrDefaultAsync(x => x.ItemName == name);

                if (findItem == null)
                {
                    Console.WriteLine($"{name} not exists");
                    return;
                }

                _ = db.Items.Remove(findItem);
                _ = await db.SaveChangesAsync();
            });

            command.AddCommand(commandRemove);
        }

        private static void RegisterCommandList(Command command)
        {
            var commandList = new Command("list", "List all items");
            commandList.Handler = CommandHandler.Create(async () =>
            {
                using var db = new ApplicationDbContext();
                var items = await db.Items.ToListAsync();
                var consoleTable = new ConsoleTable("Name", "Version", "Latest Version");
                items.ForEach(x =>
                {
                    consoleTable.AddRow(x.ItemName, x.ItemVer, x.ItemLatestVer);
                });
                consoleTable.Write();
            });

            command.AddCommand(commandList);
        }

        private static void RegisterCommandFetch(Command command)
        {
            Command commandFetch = new("fetch", "Fetch each item's version");
            commandFetch.Handler = CommandHandler.Create(async () =>
            {
                var consoleTable = new ConsoleTable("Name", "Version", "Latest Version");

                using var db = new ApplicationDbContext();
                var items = await db.Items.ToListAsync();
                var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
                foreach (var item in items)
                {
                    var response = await client.GetAsync($"{item.ItemUrl.TrimEnd('/')}/releases/latest");

                    item.ItemLatestVer = response.Headers.Location.ToString().Version();

                    if (item.ItemLatestVer != item.ItemVer)
                    {
                        consoleTable.AddRow(item.ItemName, item.ItemVer, item.ItemLatestVer);
                    }
                }
                client.Dispose();

                _ = db.SaveChangesAsync();

                consoleTable.Write();
            });

            command.AddCommand(commandFetch);
        }

        private static void RegisterCommandPull(Command command)
        {
            var commandPull = new Command("pull", "Pull the latest version");

            var optionName = new Option<string>("-name", "Display name or project name") { IsRequired = true };
            commandPull.AddOption(optionName);

            commandPull.Handler = CommandHandler.Create<string>(async name =>
            {
                using var db = new ApplicationDbContext();
                var findItem = await db.Items.FirstOrDefaultAsync(x => x.ItemName == name);

                if (findItem == null)
                {
                    Console.WriteLine($"{name} not exists");
                    return;
                }

                var client = new HttpClient();
                var response = await client.GetAsync($"{findItem.ItemUrl.TrimEnd('/')}/releases/latest");
                var htmlContent = await response.Content.ReadAsStringAsync();
                client.Dispose();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);
                var releaseNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class,'release')]");
                var assetNodes = releaseNode.SelectNodes("//details/div[contains(@class,'Box')]//div[contains(@class, 'd-flex')]");

                var consoleTable = new ConsoleTable("Index", "File", "Size");
                var files = new (string Name, string Url)[assetNodes.Count];
                for (int i = 0; i < assetNodes.Count; i++)
                {
                    var node = assetNodes[i];
                    var aNode = node.SelectSingleNode("a");
                    var url = aNode.Attributes["href"].Value;
                    var fileName = aNode.ChildNodes["span"].InnerText;
                    var size = node.SelectSingleNode("small").InnerText;

                    consoleTable.AddRow(i + 1, fileName, size);
                    files[i] = (fileName, url);
                }

                consoleTable.Write();

                var releaseVersion = releaseNode.SelectSingleNode("div//svg[contains(@class,'octicon-tag')]").ParentNode.Attributes["title"].Value;
                Console.Write($" Version: {releaseVersion}");
                var releaseTime = releaseNode.SelectSingleNode("div[contains(@class,'release-main-section')]/div[@class='release-header']//relative-time").Attributes["datetime"].Value;
                Console.WriteLine($" - released at {DateTime.Parse(releaseTime).ToLocalTime()}");
                Console.WriteLine();

                findItem.ItemLatestVer = releaseVersion;
                findItem.ItemLatestReleasedAt = DateTime.Parse(releaseTime);
                _ = await db.SaveChangesAsync();

                Console.WriteLine("Enter the index to download file or enter \"exit\".");

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();

                    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (!int.TryParse(input, out var index))
                    {
                        Console.WriteLine($"Err: \"{input}\" not a number");
                        continue;
                    }

                    if (index < 1 || index > assetNodes.Count)
                    {
                        Console.WriteLine($"Err: \"{input}\" out of the range");
                        continue;
                    }

                    var (Name, Url) = files[index - 1];
                    FileDownloader downloader = new();
                    await downloader.GetAsync(Name, Url);

                    findItem.ItemVer = releaseVersion;
                    _ = await db.SaveChangesAsync();
                }
            });

            command.AddCommand(commandPull);
        }
    }
}
