using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.PullRequests
{
    [Command("pullrequest","pull-request","pr")]
    [Subcommand(typeof(PullRequestReadCommand))]
    public class PullRequestCommand : BaseCommand
    {
        
    }
}