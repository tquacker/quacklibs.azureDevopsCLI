using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("configure", "configuration", "config", Description = "Configure options")]
    [Subcommand(typeof(ConfigureReadCommand))]
    [Subcommand(typeof(ConfigureCreateUpdateCommand))]
    [Subcommand(typeof(ConfigureDeleteCommand))]
    internal class ConfigureCommand : BaseCommand
    {
    }
}