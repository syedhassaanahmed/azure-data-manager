using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DataManager.Options;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using System;

namespace DataManager.Services
{
    public class DataFactoryService
    {
        private readonly AzureADOptions _azureAdOptions;
        private readonly DataFactoryOptions _dataFactoryOptions;
        private readonly DataFactoryManagementClient _dataFactoryClient;

        public string SubscriptionId => _dataFactoryOptions.SubscriptionId;
        public string ResourceGroup => _dataFactoryOptions.ResourceGroup;

        public DataFactoryService(IOptions<AzureADOptions> azureAdOptions, IOptions<DataFactoryOptions> dataFactoryOptions)
        {
            _azureAdOptions = azureAdOptions.Value;
            _dataFactoryOptions = dataFactoryOptions.Value;
            _dataFactoryClient = CreateDataFactoryClientAsync().Result;
        }

        public async Task<string> GetAuthenticationToken(string authority, string resource, string scope = "")
        {
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(_azureAdOptions.ClientId, _azureAdOptions.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        private async Task<DataFactoryManagementClient> CreateDataFactoryClientAsync()
        {
            var accessToken = await GetAuthenticationToken($"https://login.windows.net/{_azureAdOptions.Domain}", 
                "https://management.azure.com/");
            var cred = new TokenCredentials(accessToken);
            return new DataFactoryManagementClient(cred) { SubscriptionId = _dataFactoryOptions.SubscriptionId };
        }

        public async Task UpsertAsync(string name, LinkedServiceResource resource)
        {
            await _dataFactoryClient.LinkedServices.CreateOrUpdateAsync(_dataFactoryOptions.ResourceGroup,
                    _dataFactoryOptions.Name, name, resource);
        }

        public async Task UpsertAsync(string name, DatasetResource resource)
        {
            await _dataFactoryClient.Datasets.CreateOrUpdateAsync(_dataFactoryOptions.ResourceGroup,
                    _dataFactoryOptions.Name, name, resource);
        }

        public async Task UpsertAsync(string name, PipelineResource resource)
        {
            await _dataFactoryClient.Pipelines.CreateOrUpdateAsync(_dataFactoryOptions.ResourceGroup,
                    _dataFactoryOptions.Name, name, resource);
        }

        public async Task UpsertAsync(string name, TriggerResource resource)
        {
            await _dataFactoryClient.Triggers.CreateOrUpdateAsync(_dataFactoryOptions.ResourceGroup,
                    _dataFactoryOptions.Name, name, resource);
        }
    }
}
