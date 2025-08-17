using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems;

[Command("read", "r", Description = "List one or multiple work items from a team or project")]
internal class WorkItemReadCommand : BaseCommand
{

    [Option("-l|--list")]
    public bool ShowAll { get; set; }

    [Option("-a|--assignedTo|--for")]
    public string AssignedTo { get; set; } = "@me"; //options: @me, @all, @name

    private readonly AppOptionsService _appOptions;
    private readonly AzureDevopsService _azureDevops;

    public WorkItemReadCommand(IOptions<AppOptionsService> appOptions, AzureDevopsService azureDevops)
    {
        _appOptions = appOptions.Value;
        _azureDevops = azureDevops;
    }

    public override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {

        var wiql = new Wiql
        {
            Query = $@"
        SELECT [System.Id], [System.WorkItemType], [System.Title], [System.State]
        FROM WorkItems
        WHERE [Source.WorkItemType] = 'Feature' 
          AND [Source.TeamProject] = '{_appOptions.Defaults.Project}'
          AND [Source.AssignedTo] = '{AssignedTo}'
          AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
          AND [Target.WorkItemType] = 'Task'
        ORDER BY [System.ChangedDate] DESC
        MODE (Recursive)"
        };


        var result = await _azureDevops.GetClient<WorkItemTrackingHttpClient>().QueryByWiqlAsync(wiql);
        foreach (var item in result.Columns)
        {
            Console.WriteLine(item.Name, item.Url, item.ReferenceName);
        }

        return ExitCodes.Ok;
    }
}
