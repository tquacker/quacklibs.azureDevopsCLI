using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("read", "r", Description = "read configuration values")]
    internal class ConfigureReadCommand : BaseCommand
    {
        [Option("--projects")] 
        public bool ShowProjects { get; set; }

        private readonly AppOptionsService _settings;
        private readonly AzureDevopsService _azureDevops;

        public ConfigureReadCommand(AppOptionsService settings, AzureDevopsService azDevopsService)
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
                var result = _settings.GetCurrentConfig();

                var table = new TableBuilder<AppOptionKeyValue>()
                            .WithColumn(name: "Name", valueSelector: new(e => e.Name, TableColor.Skyblue))
                            .WithColumn(name: "Value", valueSelector: new(e => e.Value?.ToString()))
                            .WithRows(result)
                            .WithOptions(e => e.LeftAligned())
                            .Build();

                AnsiConsole.Write(table);
            }

            return ExitCodes.Ok;
        }

        private async Task ShowAvailableProjects()
        {
            var projects = await _azureDevops.GetClient<ProjectHttpClient>()
                                             .GetProjects(stateFilter: ProjectState.WellFormed);

            var projectsTable = new TableBuilder<TeamProjectReference>()
                                .WithColumn("id", new(e => e.Id.ToString()))
                                .WithColumn("url", new(e => e.Url))
                                .WithColumn("name", new(e => e.Name))
                                .WithRows(projects.ToList() ?? [])
                                .Build();
            
            AnsiConsole.Write(projectsTable);
        }
    }
}