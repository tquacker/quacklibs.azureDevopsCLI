namespace Quacklibs.AzureDevopsCli.Core.Behavior
{
    public static class WorkItemLink
    {
        public static string ToWorkItemUrl(this WorkItemLinkType type)
            => $"{type.organizationUrl}/{type.projectName.Encode()}/_workitems/edit/{type.workItemId}".AsUrlMarkup("link");
    }

    public static class AnsiConsoleExtensions
    {
        public static string AsUrlMarkup(this string url, string displayText = "link")
            => $"[Link={url}]{displayText}[/]";
    }
}