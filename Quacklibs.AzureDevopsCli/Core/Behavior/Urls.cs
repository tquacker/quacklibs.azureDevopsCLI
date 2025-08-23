namespace Quacklibs.AzureDevopsCli.Core.Behavior
{
    public static class Urls
    {
        public static string Encode(this string input) => Uri.EscapeDataString(input);
    }
}