using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems
{
    [Command("create", "-c")]
    internal class WorkItemCreateCommand : BaseCommand
    {
        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            throw new NotImplementedException();
        }
    }
}
