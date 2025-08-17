using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Quacklibs.AzureDevopsCli
{
    internal class AzureDevopsService
    {
        private readonly VssConnection _connection;
        private readonly WorkItemTrackingHttpClient _client;

        public AzureDevopsService(string organizationUrl, string pat)
        {
            _connection = CreatePATConnection(organizationUrl, pat);
            //see https://github.com/MicrosoftDocs/vsts-rest-api-specs
            _client = _connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public T GetClient<T>() where T : IVssHttpClient => _connection.GetClient<T>();
        
        
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
