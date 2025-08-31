using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;
using System.ComponentModel.DataAnnotations;


namespace Quacklibs.AzureDevopsCli.Commands.WorkItems;

/// <summary>
/// Update an workitem.
/// Features:
/// Update state
/// Update Project
/// Update iteration to: Current iteration | backlog
/// 
/// </summary>
[Command("update", "u", Description = "Update a work items from a team or project")]
internal class WorkItemUpdateCommand : BaseCommand
{
    [Option("-a|--assignedTo|--for")]
    public string AssignedTo { get; set; }

    [Option("-s|--state")]
    public WorkItemState? State { get; set; } = null;

    [Option("--id|--workitemid", Description = "Id of the workitem")]
    [Required]
    public int WorkItemId { get; set; }

    [Option("-m|-c|--comment|", Description = "Comment to add to the eworkitem")]
    public string Comment { get; set; }

    [Option("-m|-c|--comment|", Description = "Comment to add to the eworkitem")]
    public string Project { get; set; }

    private readonly SettingsService _settings;
    private readonly AzureDevopsService _azureDevops;

    public WorkItemUpdateCommand(SettingsService settings, AzureDevopsService azureDevops)
    {
        _settings = settings;
        _azureDevops = azureDevops;

        Project = base.EnvironmentSettings.Project;
    }

    public override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        var projects = await _azureDevops.GetClient<ProjectHttpClient>().GetProjects();

        var selectedProject = projects.TryGetTeamProject(Project);

        var operations = new JsonPatchDocument();
        if (State != null)
        {
            // Set title
            operations.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.State", Value = State });
        }

        if (AssignedTo != null)
        {
            //todo: check if the inputted user exists. Enable the use of first names? then add a double check 
        }

        var client = _azureDevops.GetClient<WorkItemTrackingHttpClient>();

        // if (!string.IsNullOrEmpty(Comment))
        // {
        //     var commentCreate = new CommentCreate() { Text = Comment };
        //     await client.AddCommentAsync(commentCreate, project: "YourProjectName", workItemId: WorkItemId);
        // }
        // // Add the comment
        //
        // var requestedFields = new[] { "System.Id", "System.WorkItemType", "System.State", "System.Title", "System.TeamProject" };
        // var ids = result.WorkItems.Select(e => e.Id);
        // var workItems = await _azureDevops.GetClient<WorkItemTrackingHttpClient>()
        //                                   .GetWorkItemsAsync(ids, fields: requestedFields);

        // var table = TableBuilder<WorkItem>
        //             .Create()
        //             .WithTitle("WorkItems")
        //             .WithColumn("id", new(e => e.Id.ToString()))
        //             .WithColumn("title", new(e => e.Fields[requestedFields[3]].ToString()))
        //             .WithColumn("work item type", new(e => e.Fields[requestedFields[1]].ToString()))
        //             .WithColumn("state", new(e => e.Fields[requestedFields[2]].ToString()))
        //             .WithColumn("teamProject", new(e => e.Fields[requestedFields[4]].ToString()))
        //             .WithColumn("link", new(e => $"{new WorkItemLinkType(_appOptions.Defaults.OrganizationUrl, e.Fields[requestedFields[4]]?.ToString() ?? "", e.Id).ToWorkItemUrl()}"))
        //             .WithRows(workItems)
        //             .Build();

        // AnsiConsole.Write(table);

        return ExitCodes.Ok;
    }
}