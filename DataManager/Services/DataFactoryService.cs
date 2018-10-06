using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DataManager.Options;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;

namespace DataManager.Services
{
    public class DataFactoryService
    {
        private readonly AzureADOptions _azureAdOptions;
        private readonly DataFactoryOptions _dataFactoryOptions;
        private readonly DataFactoryManagementClient _dataFactoryClient;

        public DataFactoryService(IOptions<AzureADOptions> azureAdOptions, IOptions<DataFactoryOptions> dataFactoryOptions)
        {
            _azureAdOptions = azureAdOptions.Value;
            _dataFactoryOptions = dataFactoryOptions.Value;

            _dataFactoryClient = CreateDataFactoryClientAsync().Result;
        }

        private async Task<DataFactoryManagementClient> CreateDataFactoryClientAsync()
        {
            var context = new AuthenticationContext($"https://login.windows.net/{_azureAdOptions.Domain}");
            var cc = new ClientCredential(_azureAdOptions.ClientId, _azureAdOptions.ClientSecret);
            var result = await context.AcquireTokenAsync("https://management.azure.com/", cc);
            var cred = new TokenCredentials(result.AccessToken);
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
