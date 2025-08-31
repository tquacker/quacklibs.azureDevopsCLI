﻿using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Quacklibs.AzureDevopsCli.Services;

namespace Quacklibs.AzureDevopsCli
{
    public class AzureDevopsService
    {
        private readonly SettingsService _settingsService;
        private Lazy<VssConnection> _connection 
            => new(() => CreatePATConnection(_settingsService.Settings.CurrentEnv().OrganizationUrl, _settingsService.Settings.CurrentEnv().PAT));


        public AzureDevopsService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        
        public T GetClient<T>() where T : IVssHttpClient
        {
            if(string.IsNullOrEmpty(_settingsService.Settings.CurrentEnv().OrganizationUrl))
                AnsiConsole.Write($"Unable to create a connection, no {nameof(EnvironmentConfiguration.OrganizationUrl)} defined");
            if(string.IsNullOrEmpty(_settingsService.Settings.CurrentEnv().PAT))
                AnsiConsole.Write($"Unable to Authorize to azure devops, no {nameof(EnvironmentConfiguration.PAT)} defined");
            
            return _connection.Value.GetClient<T>();
        }


        /// <summary>
        /// Personal Access Token authentication (legacy - use modern auth instead)
        /// </summary>
        public static VssConnection CreatePATConnection(string organizationUrl, string personalAccessToken)
        {
            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            return new VssConnection(new Uri(organizationUrl), credentials);
        }
    }
}
