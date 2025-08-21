using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Services;


namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("create", "update", Description = "Create or update an configuration option")]
    internal class ConfigureCreateUpdateCommand : BaseCommand
    {
        [Option("-o|--organization|--organizationurl", Description = "Set the azure devops organization url. Will usually be in the format 'https://dev.azure.com/[yourOrganization]")]
        public string? OrganizationUrl { get; set; }

        [Option("-p|--project|--team", Description = "Set the default azure devops project or team")]
        public string? Project { get; set; }

        [Option("--pat", Description= "Set the Personal access token that is used to connect to azure devops")]
        public string? Pat { get; set; }
        
        [Option("--user|--email|--useremail", Description= "Set the user")]
        public string UserEmail { get; set; }

        private readonly AppOptionsService _settings;
        private readonly ICredentialStorage _credentialStore;
        private readonly ConfigureReadCommand _readCommand;

        public ConfigureCreateUpdateCommand(AppOptionsService settings, ICredentialStorage credentialStore, ConfigureReadCommand readCommand)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _credentialStore = credentialStore;
            _readCommand = readCommand;
        }

        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (!string.IsNullOrEmpty(Project))
            {
                _settings.Defaults.Project = Project;
            }
            if (!string.IsNullOrEmpty(OrganizationUrl))
            {
                _settings.Defaults.OrganizationUrl = OrganizationUrl;
            }
            if (!string.IsNullOrEmpty(UserEmail))
            {
                _settings.Defaults.UserEmail = UserEmail;
            }
            if (!string.IsNullOrEmpty(Pat))
            {
                _settings.Defaults.PAT = Pat;
                //TODO
                // _credentialStore.set(new PersonalAccessToken(Pat));
                // Console.WriteLine("Personal access token saved to secure storage");
            }
            
            // var profile = await profileClient.GetProfileAsync(new ProfileQueryContext(AttributesScope.Core, CoreProfileAttributes.All));
            //
            _settings.Save();

            Console.WriteLine("Current settings:");
            _readCommand.OnExecuteAsync(app);

            return Task.FromResult(ExitCodes.Ok);
        }
    }
}
