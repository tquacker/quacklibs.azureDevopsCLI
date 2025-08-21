using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;
using System.Globalization;

namespace Quacklibs.AzureDevopsCli.Commands.ReleaseNotes;

[Command("create", "c")]
internal class ReleaseNoteCreateCommand : BaseCommand
{
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
    private readonly DefaultParameters _opts;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
    private readonly AzureDevopsService _service;

    [Option("-p|-project")]
    public string Project { get; set; }

    public ReleaseNoteCreateCommand(AppOptionsService opts, AzureDevopsService service)
    {
        _opts = opts.Defaults;
        _service = service;

        Project = _opts.Project;

        if (!string.IsNullOrEmpty(Project))
        {
            Console.WriteLine($"\n Project set to default: {Project} \n");
        }
    }

    public override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        await GetMergedPRs(DateTime.Now.AddDays(-100));
        await GetClosedWorkItemsAsync(DateTime.Now.AddDays(-100));

        return ExitCodes.Ok;

    }
    
    

    [SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "<Pending>")]
    private async Task GetClosedWorkItemsAsync(DateTime fromDate)
    {
        var fromDate2 = fromDate.ToString("dd-MM-yyyy").Encode();
        //Query that grabs all of the Work Items marked "Done" in the last 14 days
        Wiql wiql = new()
        {
            Query = $@"
                Select [State], [Title]
                From WorkItems
                Where ([System.State] = 'Resolved' Or [System.State] = 'Closed')
                And [System.CreatedDate] > '{fromDate2}'"
        };


        var projects = await _service.GetClient<ProjectHttpClient>().GetProjects();

        var builder = new TableBuilder<TeamProjectReference>().WithColumn("name", new(e => e.Name)).WithRows(projects);
        
        AnsiConsole.Write(builder.Build());
        

        WorkItemQueryResult workItemQueryResult = await _service.GetClient<WorkItemTrackingHttpClient>()
                                                                .QueryByWiqlAsync(wiql);

        if (workItemQueryResult.WorkItems.Any())
        {
            List<int> list = [];
            foreach (var item in workItemQueryResult.WorkItems)
            {
                list.Add(item.Id);
            }

            //Extract desired work item fields
            string[] fields = ["System.Id", "System.Title"];
            var workItems = _service.GetClient<WorkItemTrackingHttpClient>()
                                    .GetWorkItemsAsync(list, fields, workItemQueryResult.AsOf).Result;

            //Format Work Item info into text
            string txtWorkItems = string.Empty;
            foreach (var workItem in workItems)
            {
                txtWorkItems += $"\n 1. #{workItem.Id}-{workItem.Fields["System.Title"]}";
            }
        }
    }

    public async Task GetMergedPRs(DateTime releaseSpan)
    {
        var gitClient = _service.GetClient<GitHttpClient>();

        using (gitClient)
        {
            //Get first repo in project
            var releaseRepo = (await gitClient.GetRepositoriesAsync())[0];


            var searchCriteria = new GitPullRequestSearchCriteria
            {
                TargetRefName = "refs/heads/main",
                Status = PullRequestStatus.Completed,
                MaxTime = releaseSpan,
                QueryTimeRangeType = PullRequestTimeRangeType.Closed
                //there is a min time and max time that can be included in the query to make it more performant,
                //though it's not yet clear to me how that filters should be interpreted. So, it's a todo for now

            };

            //Grabs all completed PRs merged into master branch
            var prs = await (gitClient.GetPullRequestsAsync(releaseRepo.Id, searchCriteria)) ?? [];

            var closedPrsInRange = prs.Where(e => e.ClosedDate >= releaseSpan)
                                       .Select((pull, index) => $"\n {index + 1}. #{pull.PullRequestId} - {pull.Title}");

            foreach (var item in closedPrsInRange)
            {
                Console.WriteLine(item);
            }
        }
    }
}