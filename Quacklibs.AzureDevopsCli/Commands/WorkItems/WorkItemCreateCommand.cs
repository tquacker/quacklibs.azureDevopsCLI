using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models.Process;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Quacklibs.AzureDevopsCli.Services;
using System.ComponentModel.DataAnnotations;
using WorkItemType = Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models.Process.WorkItemType;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems
{
    [Command("create", "-c")]
    internal class WorkItemCreateCommand : BaseCommand
    {
        private readonly AzureDevopsService _service;
        private readonly EnvironmentConfiguration _options;

        [Option("-a|--assignedTo|--for")]
        public string AssignedTo { get; set; }
        
        [Option("-t|--title")]
        [Required]
        public string Title { get; set; }

        [Option("-d|--description|--des")]
        public string Description { get; set; } = string.Empty;
        
        [Option("--id|--parentid")]
        [Required]
        public int ParentId { get; set; }

        [Option("--state")] 
        public WorkItemState State { get; set; } = WorkItemState.New;

        [Option("--type")]
        public WorkItemKind WorkItemType { get; set; } = WorkItemKind.Task;
 
        public WorkItemCreateCommand(AzureDevopsService service, SettingsService options)
        {
            _service = service;
            AssignedTo = base.EnvironmentSettings.UserEmail;
        }
        
        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var witClient = _service.GetClient<WorkItemTrackingHttpClient>();
            
            var patchDocument = new JsonPatchDocument
            {
                // Set title
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = Title
                },
                // Description
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = Description
                },
                // Link to parent
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        //create a link from the task to the parent
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{_options.OrganizationUrl}/_apis/wit/workItems/{ParentId}",
                        attributes = new { comment = "Linked as child task" }
                    }
                },
            };
            if (!string.IsNullOrEmpty(AssignedTo))
            {
                patchDocument.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = AssignedTo
                });
            }

            // Get the parent work item. Adding this will ensure that the task get's shows on the current board
            var parentWorkItem = await witClient.GetWorkItemAsync(ParentId, new[] { "System.IterationPath" });
            var iterationPath = parentWorkItem.Fields["System.IterationPath"]?.ToString();

            //add the iteration path from the parent
            if (!string.IsNullOrEmpty(iterationPath))
            {
                 patchDocument.Add(new JsonPatchOperation
                            {
                                Operation = Operation.Add, 
                                Path = "/fields/System.IterationPath",
                                Value = iterationPath
                            });
            }

            // Create the task
            var createdWorkItem = await witClient.CreateWorkItemAsync(patchDocument, _options.DefaultProject, type:WorkItemType.ToString());

            if (createdWorkItem == null)
            {
                Console.WriteLine("Failed to create workitem");
                return ExitCodes.Ok;
            }

            var uri = _options.ToWorkItemUrl(createdWorkItem.Id.Value, _options.DefaultProject);
            
            AnsiConsole.WriteLine($"\n created {createdWorkItem.Id}. Type: {this.WorkItemType.ToString()}");
            AnsiConsole.Write(uri, new Style(foreground: Color.Blue));
      
            return ExitCodes.Ok;
        }
    }
}
