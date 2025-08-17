using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.ReleaseNotes
{
    [Command("releasenote", "realease-note", "rn")]
    [Subcommand(typeof(ReleaseNoteCreateCommand))]
    internal class ReleaseNoteCommand : BaseCommand
    {
    }
}

