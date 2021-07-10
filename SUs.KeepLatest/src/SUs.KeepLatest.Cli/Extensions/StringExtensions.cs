namespace SUs.KeepLatest.Cli.Extensions
{
    public static class StringExtensions
    {
        public static string Version(this string input)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.Contains('/'))
            {
                return string.Empty;
            }

            return input.Split('/', System.StringSplitOptions.RemoveEmptyEntries)[^1];
        }
    }
}
