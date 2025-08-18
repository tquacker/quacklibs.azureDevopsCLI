using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Services;


namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("delete", Description = "Delete all configured values")]
    internal class ConfigureDeleteCommand : BaseCommand
    {
        private readonly AppOptionsService _settings;
        private readonly ICredentialStorage _credentialStore;

        public ConfigureDeleteCommand(AppOptionsService settings, ICredentialStorage credentialStore)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _credentialStore = credentialStore;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var isOk = await AnsiConsole.ConfirmAsync("This will delete all configuration's. Do you wish to continue? ");

            if (!isOk)
                return ExitCodes.Ok;

            _settings.Delete();
           //_credentialStore.Delete();

            return ExitCodes.Ok;
        }
    }
}