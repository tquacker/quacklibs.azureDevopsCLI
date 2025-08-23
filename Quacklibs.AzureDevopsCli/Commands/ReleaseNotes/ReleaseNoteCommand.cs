using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.ReleaseNotes
{
    [Command("releasenote", "release-note", "rn", Description = "Create release notes for a project")]
    [Subcommand(typeof(ReleaseNoteCreateCommand))]
    internal class ReleaseNoteCommand : BaseCommand
    {
    }
}

