using System.CommandLine;

namespace SUs.KeepLatest.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            return RootCommandBuilder.Build().InvokeAsync(args).Result;
        }
    }
}
