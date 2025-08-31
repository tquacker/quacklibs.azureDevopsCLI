using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;


namespace Quacklibs.AzureDevopsCli.Commands.Login
{
    [Command("login")]
    internal class LoginCommand : BaseCommand
    {
        internal const string ConfigurePatHelpText = "azdo login --pat [yourpat]";

        [Option("--pat", Description = "Set the Personal access token that is used to connect to azure devops")]
        [Required]
        public string? Pat { get; set; }

        public string? Environment { get; set; }

        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            EnvironmentSettings.PAT = Pat;
            base.SettingsService.Save(Settings);

            AnsiConsole.WriteLine("Pat saved");

            return base.OnExecuteAsync(app);
        }
    }
}
