using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Quacklibs.AzureDevopsCli.Commands.Configure;
using Quacklibs.AzureDevopsCli.Commands.PullRequests;
using Quacklibs.AzureDevopsCli.Commands.ReleaseNotes;
using Quacklibs.AzureDevopsCli.Commands.SprintPlanning;
using Quacklibs.AzureDevopsCli.Commands.WorkItems;
using Quacklibs.AzureDevopsCli.Services;
using System.Reflection;


namespace Quacklibs.AzureDevopsCli
{
    [Command(Name = "azdev", Description = "an toolbox with options to make working with azure devops easier, quicker and better. ")]
    [Subcommand(typeof(ConfigureCommand))]
    [Subcommand(typeof(WorkItemCommand))]
    [Subcommand(typeof(ReleaseNoteCommand))]
    [Subcommand(typeof(PullRequestCommand))]
    [Subcommand(typeof(SprintPlanningCommand))]
    [HelpOption]
    internal class Program
    {
       public static int Main(string[] args)
        {
            WriteWelcomeMessage();

            var services = new ServiceCollection()
             .AddSingleton<IConsole>(PhysicalConsole.Singleton)
             .AddSingleton<AppOptionsService>()
             .AddScoped<AzureDevopsService>()
             .AddScoped<ICredentialStorage, CredentialStorage>()
             .AddScoped<ConfigureReadCommand>()
           .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
        
            return app.Execute(args);
        }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify a subcommand.");
            console.WriteLine();
            app.ShowHelp();
            return 1;
        }

        private static void WriteWelcomeMessage()
        {
            var versionString = Assembly.GetEntryAssembly()?
                               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                               .InformationalVersion
                               .ToString();
            
            Console.WriteLine($"Starting auzure devops cli, v{versionString}");
        }
    }
}
