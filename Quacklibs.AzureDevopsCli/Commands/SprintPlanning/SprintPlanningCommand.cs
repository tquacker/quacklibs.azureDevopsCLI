using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.SprintPlanning
{
    [Command("planning", "sprintplanning", "sprint-planning", Description = "Bulk operations on boards")]
    [Subcommand(typeof(SprintPlanningUpdateCommand))]
    internal class SprintPlanningCommand : BaseCommand
    {
        public SprintPlanningCommand()
        {
        }
    }
}
