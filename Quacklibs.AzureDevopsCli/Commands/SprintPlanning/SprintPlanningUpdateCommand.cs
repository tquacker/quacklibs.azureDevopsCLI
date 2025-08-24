using McMaster.Extensions.CommandLineUtils;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Quacklibs.AzureDevopsCli.Services;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using System.ComponentModel.DataAnnotations;
using Quacklibs.AzureDevopsCli.Core.Behavior;


namespace Quacklibs.AzureDevopsCli.Commands.SprintPlanning
{
    [Command(name: "update", Description = "Move all active items from active iteration to the new iteration")]
    internal class SprintPlanningUpdateCommand : BaseCommand
    {
        private readonly AppOptionsService options;

        public AzureDevopsService _service { get; }

        [Option("--team|--project|--for")]
        [Required]
        string Project { get; set; }


        public SprintPlanningUpdateCommand(AzureDevopsService service, AppOptionsService options)
        {
            _service = service;
            this.options = options;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var projects = await _service.GetClient<ProjectHttpClient>().GetProjects();

            var selectedProject = projects.TryGetTeamProject(Project);

            if (selectedProject == null)
            {
                Console.WriteLine("no matching project found");
                return ExitCodes.Ok;
            }

            var client = _service.GetClient<WorkHttpClient>();

            var teamContext = new TeamContext(selectedProject.Id);

            var iterations = await client.GetTeamIterationsAsync(teamContext);
            iterations = iterations.Where(e => e.Attributes.StartDate > DateTime.Now.AddDays(-50)).ToList();

            var pastIterations = iterations.Where(e => e.Attributes.TimeFrame == TimeFrame.Past || e.Attributes.TimeFrame == TimeFrame.Current);
            var futureIterations = iterations.Where(e => e.Attributes.TimeFrame == TimeFrame.Current || e.Attributes.TimeFrame == TimeFrame.Future);

            Func<TeamSettingsIteration, string> displayString = e => $"{e.Name}. Ends: {e.Attributes.FinishDate.Value.ToString(Defaults.DateFormat)}";


            var fromPrompt = new SelectionPrompt<TeamSettingsIteration>()
           .Title("Select [green]From[/]:")
           .PageSize(10)
           .AddChoices(pastIterations.ToArray())
           .UseConverter(displayString);


            var toPrompt = new SelectionPrompt<TeamSettingsIteration>()
                .Title("Select [blue]To[/]:")
                .PageSize(10)
                .AddChoices(futureIterations.ToArray())
                .UseConverter(displayString);


            var fromIteration = AnsiConsole.Prompt(fromPrompt);
            var toIteration = AnsiConsole.Prompt(toPrompt);

            var wiql = new Wiql()
            {
                Query = $@"
        SELECT [System.Id], [System.Title], [System.State], [System.WorkItemType]
        FROM WorkItems
        WHERE [System.IterationPath] = '{fromIteration.Path}'
        AND [System.State] IN ('New', 'Active')
        ORDER BY [System.Id]"
            };

            var workItemClient = _service.GetClient<WorkItemTrackingHttpClient>();

            var workItemsInCurrentIteration = await workItemClient.QueryByWiqlAsync(wiql, selectedProject.Id);

            var workitemIdsInCurrentIteration = workItemsInCurrentIteration.WorkItems.Select(e => e.Id);

            var workItemsToMove = await workItemClient.GetWorkItemsAsync(workitemIdsInCurrentIteration, expand: WorkItemExpand.Fields);


            var table = TableBuilder<WorkItem>
                .Create()
                .WithTitle("Workitems that will be moved")
                .WithColumn("Id", new ColumnValue<WorkItem>(e => e.Id.ToString()))
                .WithColumn("Title", new(e => e.Fields[AzureDevopsFields.WorkItemTitle].ToString()))
                .WithColumn("url", new(e => e.Url.AsUrlMarkup("link")))
                .WithRows(workItemsToMove)
                .Build();

            AnsiConsole.Write(table);

            bool isConfirmed = AnsiConsole.Confirm($"Do you wish to move these workitems to iteration {toIteration.Name}");

            if (!isConfirmed)
                return ExitCodes.Ok;

            var jsonPatchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = toIteration.Path
                }
            };

            foreach (var workItemId in workItemsToMove.Select(e => e.Id).Where(e => e != null))
            {
                await workItemClient.UpdateWorkItemAsync(jsonPatchDocument, workItemId!.Value);
            }

            return ExitCodes.Ok;
        }
    }
}
