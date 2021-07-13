using System.CommandLine;
using SUs.KeepLatest.Cli.DataAccess;

namespace SUs.KeepLatest.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            ApplicationDbMigrator.MigrateAsync().Wait();

            return RootCommandBuilder.Build().InvokeAsync(args).Result;
        }
    }
}
