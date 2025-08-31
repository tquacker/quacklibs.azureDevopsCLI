using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Commands.PullRequests;
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
        
        [Option("--env|--environment", Description= "create a new environment")]
        public string Environment { get; set; }

        private readonly ICredentialStorage _credentialStore;
        private readonly ConfigureReadCommand _readCommand;

        public ConfigureCreateUpdateCommand(ICredentialStorage credentialStore, ConfigureReadCommand readCommand)
        {
            _credentialStore = credentialStore;
            _readCommand = readCommand;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            base.OnExecuteAsync(app);

            if (!string.IsNullOrEmpty(Environment))
            {
                if (base.Settings.TryAddEnvironment(Environment) == false)
                    return ExitCodes.Error;
            }
            
            //TODO: this should be a choice from the available projects | a walkthrough
            if (!string.IsNullOrEmpty(Project))
            {
                EnvironmentSettings.Project = Project;
            }
            if (!string.IsNullOrEmpty(OrganizationUrl))
            {
                EnvironmentSettings.OrganizationUrl = OrganizationUrl;
            }
            //TODO: this should be retrieved from the PAT
            if (!string.IsNullOrEmpty(UserEmail))
            {
                EnvironmentSettings.UserEmail = UserEmail;
            }
            if (!string.IsNullOrEmpty(Pat))
            {
                EnvironmentSettings.PAT = Pat;
                //TODO
                // _credentialStore.set(new PersonalAccessToken(Pat));
                // Console.WriteLine("Personal access token saved to secure storage");
            }
            
            // var profile = await profileClient.GetProfileAsync(new ProfileQueryContext(AttributesScope.Core, CoreProfileAttributes.All));
            //
            base.SettingsService.Save(base.Settings);

            Console.WriteLine("Current settings:");
            _readCommand.OnExecuteAsync(app);

            return ExitCodes.Ok;
        }
    }
}
