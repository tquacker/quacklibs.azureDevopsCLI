using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;

namespace Quacklibs.AzureDevopsCli.Commands.ReleaseNotes;

[Command("create", "c")]
internal class ReleaseNoteCreateCommand : BaseCommand
{
    private readonly DefaultParameters _opts;
    private readonly AzureDevopsService _service;

    [Option("-p|--project|--for")]
    public string Project { get; set; }

    public ReleaseNoteCreateCommand(AppOptionsService opts, AzureDevopsService service)
    {
        _opts = opts.Defaults;
        _service = service;

        Project = _opts.Project;
    }

    public override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        var projects = await _service.GetClient<ProjectHttpClient>().GetProjects();

        var filteredProjects = projects.FirstOrDefault(e => e.Name.StartsWith(Project, StringComparison.InvariantCultureIgnoreCase));
        var projectId = filteredProjects?.Id;
        
        Console.WriteLine($"Creating release notes for project: {filteredProjects.Name} {projectId}");
        
        await GetMergedPRs(DateTime.Now.AddDays(-100), projectId.Value);
      
        return ExitCodes.Ok;
    }

    // private async Task GetClosedWorkItemsAsync(DateTime fromDate, Guid projectId)
    // {
    //     var fromDate2 = fromDate.ToString("yyyy-MM-dd").Encode();
    //     //Query that grabs all of the Work Items marked "Done" in the last days
    //     Wiql wiql = new()
    //     {
    //         Query = $@"
    //             SELECT [System.Id]
    //             From WorkItems
    //             Where ([System.State] IN ('Resolved','Closed'))                
    //             And [System.ChangedDate] > '{fromDate2}'"
    //     };
    //     
    //     Console.WriteLine(wiql.Query);
    //
    //     var workItems = await _service.GetClient<WorkItemTrackingHttpClient>().QueryByWiqlAsync(wiql);
    //     var ids = workItems.WorkItems.Select(e => e.Id);
    //
    //     var fieldsToGet = new[] { "System.Id", "System.Title", "System.State" };
    //     var workItemWithFields = await _service.GetClient<WorkItemTrackingHttpClient>().GetWorkItemsAsync(ids, fieldsToGet);
    //
    //     if (!workItems.WorkItems.Any())
    //     {
    //         foreach (var workitem in workItemWithFields)
    //         {
    //            Console.WriteLine($"{workitem.Fields["System.Title"]}");
    //         }
    //         return;
    //     }
    // }

    public async Task GetMergedPRs(DateTime releaseDate, Guid projectId)
    {
        var gitClient = _service.GetClient<GitHttpClient>();

        using (gitClient)
        {
            //Get repo's in project
            var releaseRepos = (await gitClient.GetRepositoriesAsync(project: projectId));

            GitRepository? selectedGitRepository = null;

            if (releaseRepos.Count == 0)
                
            {
                Console.WriteLine("No repository found");
                return;
            }
            if (releaseRepos.Count == 1)
            {
                selectedGitRepository = releaseRepos.FirstOrDefault();
            }
            else if (releaseRepos.Count > 1)
            {
                var choices = releaseRepos.Select(value => value.Name).ToArray();
                
                // Ask for the user's favorite fruit
                var promptResult = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                              .Title("Multiple repos's found. Please select one")
                                              .PageSize(20)
                                              .AddChoices(choices));

                selectedGitRepository = releaseRepos.FirstOrDefault(e => e.Name == promptResult);
            }
            
            var searchCriteria = new GitPullRequestSearchCriteria
            {
                TargetRefName = "refs/heads/main",
                Status = PullRequestStatus.Completed, 
                MinTime = releaseDate, //get everything after this date
                QueryTimeRangeType = PullRequestTimeRangeType.Closed
            };

            //Grabs all completed PRs merged into master branch
            var pullRequests = await gitClient.GetPullRequestsAsync(selectedGitRepository.Id, searchCriteria) ?? [];
               
            var fieldsToGet = new[] { "System.Id", "System.Title", "System.State", "System.WorkItemType", "System.AssignedTo" };
            var workItemsclient = _service.GetClient<WorkItemTrackingHttpClient>();

            var result = new List<PullRequestWithWorkItems>();

            foreach (var pullRequest in pullRequests)
            {
                var workItemReference = await gitClient.GetPullRequestWorkItemRefsAsync(selectedGitRepository.Id, pullRequest.PullRequestId);
                var ids = workItemReference.Select(e => int.Parse(e.Id));

                var workItems = await workItemsclient.GetWorkItemsAsync(ids: ids, fields: fieldsToGet);
                result.Add(new PullRequestWithWorkItems(pullRequest, workItems));
            }

            result.Where(e => e.pr.ClosedDate >= releaseDate)
                  .Select((pull, index) => $"{index + 1}. #{pull.pr.PullRequestId} - {pull.pr.Title}, {pull.pr.Description}" + $"\n {pull.FlattenedWorkItem()}")
                  .ForEach(e => Console.WriteLine(e));
        }

    }
    public record PullRequestWithWorkItems(GitPullRequest pr, List<WorkItem> workItem)
    {
        public string FlattenedWorkItem() => string.Join("\n",  workItem.Select(e => $"{e.Fields["System.Title"]} {e.Fields["System.WorkItemType"]} {e.Fields["System.AssignedTo"]} ").ToArray());
    }
}