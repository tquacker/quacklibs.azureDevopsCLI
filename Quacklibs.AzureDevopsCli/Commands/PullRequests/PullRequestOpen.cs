using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.PullRequests
{
    [Command("open", "Open an pull request in the browser")]
    public class PullRequestOpen : BaseCommand
    {
        [Option("id")]
        public int Id { get; set; }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            // https://devops.xxx.nl/MyOrg/MyProject/_git/MyRepo/pullrequest/1234
          
            //hier iets slims voor bedenken. Je hebt het id, maar die is niet uniek per repo.  Hoe kun je mensen makkelijk een PR laten openen in de CLI? 
            
            
            // var url = {EnvironmentSettings.OrganizationUrl}.url
            return ExitCodes.Ok;
            
        }
}