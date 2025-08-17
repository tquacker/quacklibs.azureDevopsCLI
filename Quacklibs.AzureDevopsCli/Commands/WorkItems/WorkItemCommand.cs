using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems
{

    [Command("workitem", "wi")]
    [Subcommand(typeof(WorkItemReadCommand))]
    [Subcommand(typeof(WorkItemCreateCommand))]
    internal class WorkItemCommand : BaseCommand
    {
    } 
}



