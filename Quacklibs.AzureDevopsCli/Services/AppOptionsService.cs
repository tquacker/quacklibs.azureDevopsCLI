using System.Text.Json.Serialization;
using System.Text.Json;


namespace Quacklibs.AzureDevopsCli
{
    internal record AppOptionKeyValue(string Name, object? Value);

    internal class AppOptionsService
    {
        public DefaultParameters Defaults;

        private static object _FileLock = new();

        private readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true, };

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
                lock (_FileLock)
                {
                    string configurationFile = this.GetConfigurationFilePath();

                    CreateConfigFileIfNotExists(configurationFile);

                    var configFileContents = File.ReadAllText(configurationFile);

                    if (!string.IsNullOrEmpty(configFileContents))
                    {
                        Defaults = JsonSerializer.Deserialize<DefaultParameters>(configFileContents);
                    }
                    else
                    {
                        Defaults = new DefaultParameters();
                        Console.WriteLine("No configuration file found use --config -p [projectname]  -o [organizationName]");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void CreateConfigFileIfNotExists(string configurationFile)
        {
            string configurationDirectory = Path.GetDirectoryName(configurationFile);
            if (!Directory.Exists(configurationDirectory))
            {
                Directory.CreateDirectory(configurationDirectory);
            }

            if (!File.Exists(configurationFile))
            {
                File.WriteAllText(configurationFile, "{}");
            }
        }

        internal void Delete()
        {
            lock (_FileLock)
            {
                if (File.Exists(this.GetConfigurationFilePath()))
                {
                    File.Delete(this.GetConfigurationFilePath());
                    Console.WriteLine("Configuration deleted");
                }
                else
                {
                    Console.WriteLine($"No configuration file found to delete");
                }
            }
        }

        internal void Save()
        {
            string configurationFile = this.GetConfigurationFilePath();
            CreateConfigFileIfNotExists(configurationFile);

            var output = JsonSerializer.Serialize(Defaults, _options);

            File.WriteAllText(configurationFile, output);
        }
    }


    public class DefaultParameters
    {
        public string OrganizationUrl { get; set; }

        public string Project { get; set; } = "MVT - ValidatieTeruggaafBTW - Douane";
        
        public string PAT { get; set; }
        
        public string UserEmail { get; set; }
    }
}