using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands.Configure
{
    [Command("read", "r", Description = "read configuration values")]
    internal class ConfigureReadCommand : BaseCommand
    {
        private readonly AppOptionsService _settings;
        public ConfigureReadCommand(AppOptionsService settings)
        {
            _settings = settings;
        }

        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var result = _settings.GetCurrentConfig();

            var table = new TableBuilder<AppOptionKeyValue>()
                .WithColumn(name: "Name", valueSelector: new(e => e.Name, TableColor.Skyblue))
                .WithColumn(name: "Value", valueSelector: new(e => e.Value?.ToString()))
                .WithRows(result)
                .WithOptions(e => e.LeftAligned())
                .Build();

            AnsiConsole.Write(table);

            return Task.FromResult(ExitCodes.Ok);
        }
    }
}

