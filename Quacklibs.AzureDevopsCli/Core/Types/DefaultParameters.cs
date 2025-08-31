using Quacklibs.AzureDevopsCli.Core.Behavior;

namespace Quacklibs.AzureDevopsCli.Core.Types
{
    public class Settings
    {
        public string SelectedEnvironment { get; set; }
        public Dictionary<string, EnvironmentConfiguration> EnvironmentConfigurations { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

        public EnvironmentConfiguration CurrentEnv()
        {
            if (string.IsNullOrEmpty(SelectedEnvironment))
                TryAddEnvironment("Default");

            return EnvironmentConfigurations.TryGetValue(SelectedEnvironment, out var environmentSettings) ? environmentSettings : new EnvironmentConfiguration();
        }

        public bool TryAddEnvironment(string newEnvName)
        {
            if (EnvironmentConfigurations.TryGetValue(newEnvName, out var result))
            {
                Console.WriteLine("Environment already exists");
                SelectedEnvironment = newEnvName;
                return true;
            }
            else
            {
                EnvironmentConfigurations.Add(newEnvName, new EnvironmentConfiguration());
                SelectedEnvironment = newEnvName;
                Console.WriteLine($"New environment created: {SelectedEnvironment}");
                return true;
            }
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentEnv().PAT);
    }
    
    public class EnvironmentConfiguration
    {
        public string OrganizationUrl { get; set; }

        public string DefaultProject { get; set; }

        public string PAT { get; set; }

        public string UserEmail { get; set; }

        public string ToWorkItemUrl(int id, string project = "")
        {
            var workItemProject = string.IsNullOrEmpty(project) ? DefaultProject : project;
            return $"{OrganizationUrl}/{workItemProject.Encode()}/_workitems/edit/{id}";
        }
    }
}