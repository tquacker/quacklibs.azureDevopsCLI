using McMaster.Extensions.CommandLineUtils;

namespace Quacklibs.AzureDevopsCli.Commands;

[HelpOption("--help")]
public abstract class BaseCommand
{
    public virtual Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        Console.WriteLine("no parameter provided. append --help to the command see the available options");
        return Task.FromResult(ExitCodes.Ok);
    }

}
