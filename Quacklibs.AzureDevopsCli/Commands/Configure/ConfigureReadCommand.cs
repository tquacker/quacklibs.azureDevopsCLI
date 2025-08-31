using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;
using Quacklibs.AzureDevopsCli.Services;

namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("read", "r", Description = "read configuration values")]
    internal class ConfigureReadCommand : BaseCommand
    {
        [Option("--projects")]
        public bool ShowProjects { get; set; }

        private readonly SettingsService _settings;
        private readonly AzureDevopsService _azureDevops;

        public ConfigureReadCommand(SettingsService settings, AzureDevopsService azDevopsService)
        {
            _settings = settings;
            _azureDevops = azDevopsService;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (ShowProjects)
                await ShowAvailableProjects();
            else
            {
                try
                {
                    foreach (var environmentConfig in base.Settings.EnvironmentConfigurations)
                    {
                        var configOptions = _settings.GetConfig(environmentConfig.Value);
                        var title = $"Configuration - [{environmentConfig.Key}]".EscapeMarkup();
                        var table = TableBuilder<AppOptionKeyValue>
                                    .Create()
                                    .WithTitle(title)
                                    .WithColumn(name: "Name", valueSelector: new(e => e.Name))
                                    .WithColumn(name: "Value", valueSelector: new(e => e.Value?.ToString()))
                                    .WithRows(configOptions)
                                    .WithOptions(e => e.LeftAligned())
                                    .Build();

                        AnsiConsole.Write(table);
                    }
                }
                catch (Exception exception)
                {
                    return ExitCodes.Error;
                }
            }

            return ExitCodes.Ok;
        }

        private async Task ShowAvailableProjects()
        {
            var projects = await _azureDevops.GetClient<ProjectHttpClient>()
                                             .GetProjects(stateFilter: ProjectState.WellFormed);

            var projectsTable = TableBuilder<TeamProjectReference>
                                .Create()
                                .WithTitle("Projects")
                                .WithColumn("id", new(e => e.Id.ToString()))
                                .WithColumn("url", new(e => e.Url))
                                .WithColumn("name", new(e => e.Name))
                                .WithRows(projects.ToList() ?? [])
                                .Build();

            AnsiConsole.Write(projectsTable);
        }
    }
}