using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;


namespace Quacklibs.AzureDevopsCli.Commands.WorkItems;

[Command("read", "r", Description = "List one or multiple work items from a team or project")]
internal class WorkItemReadCommand : BaseCommand
{
    [Option("-a|--assignedTo|--for")]
    public string AssignedTo { get; set; }

    [Option("-s|--state")]
    public WorkItemState[] State { get; set; } = [WorkItemState.Active];

    private readonly AppOptionsService _appOptions;
    private readonly AzureDevopsService _azureDevops;

    public WorkItemReadCommand(AppOptionsService appOptions, AzureDevopsService azureDevops)
    {
        _appOptions = appOptions;
        _azureDevops = azureDevops;
    }

    public override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        string stateFilterClause = string.Empty;
        {
            var statesQuoted = State.Select(s => $"'{s}'");
            stateFilterClause = $"AND [System.State] IN ({string.Join(", ", statesQuoted)})";
        }

        string assignedToClause = "";
        if (AssignedTo is "@me" or "me")
        {
            AssignedTo = _appOptions.Defaults.UserEmail;
        }

        if (!string.IsNullOrEmpty(AssignedTo))
        {
            assignedToClause = $"AND  [System.AssignedTo] = '{AssignedTo}'";
        }

        var rawQuery = $"""
                                SELECT [System.Id], [System.WorkItemType], [System.Title], [System.State]
                                FROM WorkItems
                                WHERE [System.WorkItemType] IN ('Bug', 'Task', 'User Story', 'Feature')
                                {assignedToClause}
                                {stateFilterClause} 
                                ORDER BY [System.ChangedDate] DESC
                        """;

        var cleanedQuery = string.Join(
            Environment.NewLine,
            rawQuery
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line)));

        var wiql = new Wiql() { Query = cleanedQuery };

        Console.WriteLine(wiql.Query);

        var result = await _azureDevops.GetClient<WorkItemTrackingHttpClient>().QueryByWiqlAsync(wiql);

        var requestedFields = new[] { "System.Id", "System.WorkItemType", "System.State", "System.Title", "System.TeamProject" };
        var ids = result.WorkItems.Select(e => e.Id);
        var workItems = await _azureDevops.GetClient<WorkItemTrackingHttpClient>()
                                          .GetWorkItemsAsync(ids, fields: requestedFields);

        //Console.WriteLine(result?.WorkItemRelations?.Count());
        // Console.WriteLine(result?.WorkItems?.Count());
        Console.WriteLine(result?.Columns?.Count());

        var table = new TableBuilder<WorkItem>()
                    .WithColumn("id", new(e => e.Id.ToString()))
                    .WithColumn("title", new(e => e.Fields[requestedFields[3]].ToString()))
                    .WithColumn("work item type", new(e => e.Fields[requestedFields[1]].ToString()))
                    .WithColumn("state", new(e => e.Fields[requestedFields[2]].ToString()))
                    .WithColumn("teamProject", new(e => e.Fields[requestedFields[4]].ToString()))
                    .WithColumn("link", new(e => $"{new WorkItemLinkType(_appOptions.Defaults.OrganizationUrl, e.Fields[requestedFields[4]]?.ToString() ?? "", e.Id).ToWorkItemUrl()}"))
                    .WithRows(workItems)
                    .Build();

        AnsiConsole.Write(table);

        return ExitCodes.Ok;
    }
}