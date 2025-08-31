using McMaster.Extensions.CommandLineUtils;
using Quacklibs.AzureDevopsCli.Commands.Login;


namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("create", "update", Description = "Create or update an configuration option")]
    internal class ConfigureCreateUpdateCommand : BaseCommand
    {
        [Option("-o|--organization|--organizationurl", Description = "Set the azure devops organization url. Will be in the format 'https://dev.azure.com/[yourOrganization] when hosted in the cloud | ")]
        public string? OrganizationUrl { get; set; }

        [Option("-p|--project|--team", Description = "Set the default azure devops project or team")]
        public string? Project { get; set; }

        [Option("--pat", Description= "Set the Personal access token that is used to connect to azure devops")]
        public string? Pat { get; set; }
        
        [Option("--user|--email|--useremail", Description= "Set the user")]
        public string UserEmail { get; set; } 
        
        [Option("--env|--environment", Description= "create a new environment")]
        public string Environment { get; set; }


        private readonly ConfigureReadCommand _readCommand;

        public ConfigureCreateUpdateCommand(ConfigureReadCommand readCommand)
        {
            _readCommand = readCommand;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            await base.OnExecuteAsync(app);

            if (!base.Settings.IsAuthenticated)
            {
                Console.WriteLine($"No pat configured. Set that with {LoginCommand.ConfigurePatHelpText} ");
            }

            if (!string.IsNullOrEmpty(Environment))
            {
                base.Settings.TryAddEnvironment(Environment);
            }

            
            //TODO: this should be a choice from the available projects | a walkthrough
            if (!string.IsNullOrEmpty(Project))
            {
                EnvironmentSettings.DefaultProject = Project;
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
                
            // var profile = await profileClient.GetProfileAsync(new ProfileQueryContext(AttributesScope.Core, CoreProfileAttributes.All));
            //
            base.SettingsService.Save(base.Settings);

            Console.WriteLine("Current settings:");

            await _readCommand.OnExecuteAsync(app);

            return ExitCodes.Ok;
        }
    }
}
