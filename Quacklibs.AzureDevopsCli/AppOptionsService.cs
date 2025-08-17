using System.Text.Json.Serialization;
using System.Text.Json;


namespace Quacklibs.AzureDevopsCli
{

    internal record AppOptionKeyValue(string Name, object? Value);

    internal class AppOptionsService
    {
        public DefaultParameters Defaults;

        private readonly JsonSerializerOptions _options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        public AppOptionsService()
        {
            Load();
        }

        public string GetConfigurationFilePath()
        {
            string homeUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Join(homeUserProfile, ".devops", "config.json");
        }

        public List<AppOptionKeyValue> GetCurrentConfig()
        {
            var props = typeof(DefaultParameters).GetProperties();
            return props.Select(prop => new AppOptionKeyValue(prop.Name, prop.GetValue(Defaults))).ToList();
        }


        internal void Load()
        {
            try
            {
                string configurationFile = this.GetConfigurationFilePath();
                string configurationDirectory = Path.GetDirectoryName(configurationFile);

                if (!Directory.Exists(configurationDirectory))
                {
                    Directory.CreateDirectory(configurationDirectory);
                }
                if (!File.Exists(configurationFile))
                {
                    File.Create(configurationFile);
                }
                var configFileContents = File.ReadAllText(configurationFile);

                if (configFileContents == string.Empty || !string.IsNullOrEmpty(configFileContents))
                {
                    Defaults = JsonSerializer.Deserialize<DefaultParameters>(configFileContents);
                }
                else
                {
                    Defaults = new DefaultParameters();
                    Console.WriteLine("No configuration file found use --config -p [projectname]  -o [organizationName]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal void Delete()
        {
            if (File.Exists(this.GetConfigurationFilePath()))
            {
                File.Delete(this.GetConfigurationFilePath());
                Console.WriteLine("Configurion deleted");
            }
            else
            {
                Console.WriteLine($"No configuration file found to delete");
            }
        }

        internal void Save()
        {
            string configurationFile = this.GetConfigurationFilePath();

            string configurationDirectory = Path.GetDirectoryName(configurationFile);

            if (!Directory.Exists(configurationDirectory))
            {
                Directory.CreateDirectory(configurationDirectory);
            }

            var output = JsonSerializer.Serialize(Defaults, _options);

            Console.WriteLine("Expected output");
            Console.WriteLine(output);

            File.WriteAllText(configurationFile, output);
        }
    }


    public class DefaultParameters
    {

        public string OrganizationUrl { get; set; }

        public string Project { get; set; }
    }
}
