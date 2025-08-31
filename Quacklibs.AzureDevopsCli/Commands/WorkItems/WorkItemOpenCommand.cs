using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Quacklibs.AzureDevopsCli.Core;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;
using System.ComponentModel.DataAnnotations;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems
{
    [Command(CommandConstants.OpenCommand, Description = "Open a workitem")]
    public class WorkItemOpenCommand : BaseCommand
    {
        private readonly AzureDevopsService _service;

        [Option(CommandConstants.ProjectOptionTemplate, Description = "Project where the id is in")]
        [Required]
        public string Project { get; set; }

        [Option("--id", Description = "id of the workitem")]
        [Required]
        public int Id { get; set; }

        public WorkItemOpenCommand(AzureDevopsService service)
        {
            _service = service;

            Project = base.EnvironmentSettings.Project;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
           
            if (Project ==  base.EnvironmentSettings.Project)
            {
                var uri = $"{base.EnvironmentSettings.OrganizationUrl}/{Project.Encode()}/_workitems/edit/{Id}";
                
                Console.WriteLine($"Opening {uri}");
                Browser.Open(uri);
            }
            else
            {
                var projects = await _service.GetClient<ProjectHttpClient>().GetProjects(ProjectState.WellFormed);
                var selectedProject = projects.TryGetTeamProject(Project);
                if (selectedProject == null)
                {
                    Console.WriteLine("no project found");
                    return ExitCodes.Ok;
                }

                string uri = $"{base.EnvironmentSettings.OrganizationUrl}/{selectedProject.FullProjectName.Encode()}/_workitems/edit/{Id}";
                Console.WriteLine($"Opening {uri}");
                Browser.Open(uri);
            }

            return ExitCodes.Ok;
        }
    }
}