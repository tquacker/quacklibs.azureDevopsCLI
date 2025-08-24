using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Commands.ReleaseNotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quacklibs.AzureDevopsCli.Commands.SprintPlanning
{
    [Command("planning", Description = "Bulk operations on boards")]
    [Subcommand(typeof(SprintPlanningUpdateCommand))]
    internal class SprintPlanningCommand : BaseCommand
    {
        public SprintPlanningCommand()
        {
        }
    }
}
