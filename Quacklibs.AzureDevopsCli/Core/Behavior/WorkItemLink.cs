namespace Quacklibs.AzureDevopsCli.Core.Behavior
{
    public static class WorkItemLink
    {
        public static string ToWorkItemUrl(this WorkItemLinkType type) 
            => $"[link={type.organizationUrl}/{type.projectName.Encode()}/_workitems/edit/{type.workItemId}]Link[/]";
    }
}