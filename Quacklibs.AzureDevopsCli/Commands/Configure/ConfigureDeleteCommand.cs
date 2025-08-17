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

        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            _settings.Delete();
            _credentialStore.Delete();

            return Task.FromResult(ExitCodes.Ok);
        }
    }
}
