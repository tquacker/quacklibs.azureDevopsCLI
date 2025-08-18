using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Quacklibs.AzureDevopsCli
{
    internal class AzureDevopsService
    {
        private readonly AppOptionsService _appOptionsService;
        private Lazy<VssConnection> _connection 
            => new(() => CreatePATConnection(_appOptionsService.Defaults.OrganizationUrl, _appOptionsService.Defaults.PAT));


        public AzureDevopsService(AppOptionsService appOptionsService)
        {
            _appOptionsService = appOptionsService;
        }

        
        public T GetClient<T>() where T : IVssHttpClient
        {
            if(string.IsNullOrEmpty(_appOptionsService.Defaults.OrganizationUrl))
                AnsiConsole.Write($"Unable to create a connection, no {nameof(DefaultParameters.OrganizationUrl)} defined");
            if(string.IsNullOrEmpty(_appOptionsService.Defaults.PAT))
                AnsiConsole.Write($"Unable to Authorize to azure devops, no {nameof(DefaultParameters.PAT)} defined");
            
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
