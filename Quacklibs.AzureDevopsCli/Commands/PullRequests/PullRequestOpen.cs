using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Spectre.Console;

namespace Quacklibs.AzureDevopsCli.Commands.PullRequests
{
    [Command("open", "Open an pull request in the browser")]
    public class PullRequestOpen : BaseCommand
    {
        private readonly AzureDevopsService _service;

        [Option("id")]
        public int PullRequestId { get; set; }

        public PullRequestOpen(AzureDevopsService service)
        {
            this._service = service;
        }
        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var gitClient = _service.GetClient<GitHttpClient>();

            var pr = await gitClient.GetPullRequestByIdAsync(PullRequestId);

            if (pr == null)
            {
                AnsiConsole.WriteLine($"No pull request found for id {PullRequestId}");
                return ExitCodes.Ok;
            }

            // Build the UI link using the repository URL + "/pullrequest/" + PR id
            string prWebUrl = $"{pr.Repository.RemoteUrl}/pullrequest/{PullRequestId}";

            Console.WriteLine($"Opening {prWebUrl}");
            Browser.Open(prWebUrl);

            return ExitCodes.Ok;

        }
    }
}