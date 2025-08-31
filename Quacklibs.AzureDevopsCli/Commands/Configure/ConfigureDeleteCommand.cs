using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Services;


namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("delete", Description = "Delete all configured values")]
    internal class ConfigureDeleteCommand : BaseCommand
    {
        private readonly SettingsService _settings;
        private readonly ICredentialStorage _credentialStore;
        
        [Option("--env|--environment")]
        public string Environment { get; set; }

        public ConfigureDeleteCommand(SettingsService settings, ICredentialStorage credentialStore)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _credentialStore = credentialStore;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            bool deleteEverything = string.IsNullOrEmpty(Environment);
            var message = deleteEverything ? "This will delete all configuration's. Do you wish to continue?" : $"This will delete config for project {Environment} Do you with to continue?"; 
            var isOk = await AnsiConsole.ConfirmAsync(message);

            if (!isOk)
                return ExitCodes.Ok;

            
            if(deleteEverything)
                _settings.Delete();
            else
                _settings.Delete(Environment);
  
           //_credentialStore.Delete();

            return ExitCodes.Ok;
        }
    }
}