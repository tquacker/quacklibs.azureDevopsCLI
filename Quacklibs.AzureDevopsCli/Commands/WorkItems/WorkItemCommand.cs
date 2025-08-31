using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.WorkItems
{

    [Command("workitem", "wi", Description = "Create and read work items, add tasks etc")]
    [Subcommand(typeof(WorkItemReadCommand))]
    [Subcommand(typeof(WorkItemCreateCommand))]
    [Subcommand(typeof(WorkItemOpenCommand))]
    internal class WorkItemCommand : BaseCommand
    {
    } 
}



