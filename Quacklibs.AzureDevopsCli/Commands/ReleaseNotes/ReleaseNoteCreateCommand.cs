using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
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

    public async Task GetMergedPRs(DateTime releaseDate, Guid projectId)
    {
        var gitClient = _service.GetClient<GitHttpClient>();

        using (gitClient)
        {
            //Get repo's in project
            var releaseRepos = (await gitClient.GetRepositoriesAsync(project: projectId));

            GitRepository? selectedGitRepository = SelectGitRepository(releaseRepos);

            if (selectedGitRepository == null)

            {
                Console.WriteLine("No repository found or selected");
                return;
            }

            var searchCriteria = new GitPullRequestSearchCriteria
            {
                //TargetRefName = "refs/heads/main",
                Status = PullRequestStatus.Completed,
                MinTime = releaseDate, //get everything after this date
                QueryTimeRangeType = PullRequestTimeRangeType.Closed
            };

            //Grabs all completed PRs merged into master branch
            var pullRequests = await gitClient.GetPullRequestsAsync(selectedGitRepository.Id, searchCriteria) ?? [];

            var fieldsToGet = new[] { "System.Id", "System.Title", "System.State", "System.WorkItemType", "System.AssignedTo" };
            var workItemsclient = _service.GetClient<WorkItemTrackingHttpClient>();

            var title = $"Release notes for period {releaseDate.ToString(Defaults.DateFormat)} - {DateTime.Now.ToString(Defaults.DateFormat)}";

            var releaseNote = new ReleaseNoteType(title);


            foreach (var pullRequest in pullRequests)
            {
                var workItemReference = await gitClient.GetPullRequestWorkItemRefsAsync(selectedGitRepository.Id, pullRequest.PullRequestId);
                var ids = workItemReference.Select(e => int.Parse(e.Id));

                //this retrieves all the workitem details and their parents (e.g., feature)
                var workItems = await workItemsclient.GetWorkItemsAsync(ids: ids, fields: fieldsToGet, expand: WorkItemExpand.Relations);

                foreach (var workItem in workItems)
                {
                    var feature = await GetFeatureForWorkItemAsync(workItem);
                    var featureState = feature.Fields[AzureDevopsFields.WorkItemState];

                    //TODO

                    //releaseNote.Add(new ReleasedFeature(featureState)
                    //{
                    //    Id = feature.Id
                    //}

                }

                Console.WriteLine("In progress");
                //result.Where(e => e.pr.ClosedDate >= releaseDate)
                //      .Select((pull, index) => $"{index + 1}. #{pull.pr.PullRequestId} - {pull.pr.Title}, {pull.pr.Description}" + $"\n {pull.FlattenedWorkItem()}")
                //      .ForEach(e => Console.WriteLine(e));
            }

        }
    }

    private static GitRepository? SelectGitRepository(List<GitRepository> releaseRepos)
    {
        if (releaseRepos.Count == 1)
        {
            return releaseRepos[0];
        }
        else if (releaseRepos.Count > 1)
        {
            var choices = releaseRepos.Select(value => value.Name).ToArray();

            // Ask for the user's favorite fruit
            var promptResult = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                          .Title("Multiple repos's found. Please select one")
                                          .PageSize(20)
                                          .AddChoices(choices));

            return releaseRepos.First(e => e.Name == promptResult);
        }
        else
        {
            return null;
        }

    }

    public record PullRequestWithWorkItems(GitPullRequest pr, List<WorkItem> workItem)
    {
        public string FlattenedWorkItem() => string.Join("\n", workItem.Select(e => $"{e.Fields["System.Title"]} {e.Fields["System.WorkItemType"]} {e.Fields["System.AssignedTo"]} ").ToArray());
    }

    /// <summary>
    /// Traverses parent links until it finds a Feature (or Epic).
    /// </summary>
    public async Task<WorkItem?> GetFeatureForWorkItemAsync(WorkItem? current)
    {

        // Check if current is already a Feature
        var workItemType = current?.Fields[AzureDevopsFields.WorkItemType].ToString();
        if (workItemType == AzureDevopsWorkItemTypes.Feature || workItemType == AzureDevopsWorkItemTypes.Epic)
        {
            return current;
        }

        // Find parent relation
        var parentRelation = current?.Relations?.FirstOrDefault(r => r.Rel == AzureDevopsRefs.ParentWorkItem);

        if (parentRelation == null)
        {
            return null; // no parent found
        }


        // Extract parentId from URL
        var parentId = int.Parse(parentRelation.Url.Split('/').Last());

        // Recursively check parent
        var parent = (await GetWorkItem(parentId)).FirstOrDefault();

        return await GetFeatureForWorkItemAsync(parent);
    }


    public async Task<List<WorkItem?>> GetWorkItem(params int[] ids)
    {
        var fieldsToGet = new[] { "System.Id", "System.Title", "System.State", "System.WorkItemType", "System.AssignedTo" };

        //this retrieves all the workitem details and their parents (e.g., feature)
        var workItems = await _service.GetClient<WorkItemTrackingHttpClient>()
                                      .GetWorkItemsAsync(ids: ids, fields: fieldsToGet, expand: WorkItemExpand.Relations);

        return workItems;
    }
}