using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Quacklibs.AzureDevopsCli.Services;

namespace Quacklibs.AzureDevopsCli.Commands;

[HelpOption("--help")]
public abstract class BaseCommand
{
    protected readonly SettingsService SettingsService;
    protected readonly Settings Settings;
    protected EnvironmentConfiguration EnvironmentSettings => Settings.CurrentEnv();

    protected BaseCommand()
    {
        SettingsService = Program.ServiceLocator.GetService<SettingsService>()!;
        Settings = SettingsService!.Settings;
    }
    
    public virtual Task<int> OnExecuteAsync(CommandLineApplication app)
    {
         Console.WriteLine("no parameter provided. append --help to the command see the available options");
        return Task.FromResult(ExitCodes.Ok);
    }

}
