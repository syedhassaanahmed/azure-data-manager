using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DataManager.Options;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using System;
using System.Linq;
using System.Collections.Generic;

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

        private async Task<string> GetAuthenticationToken(string authority, string resource, string scope = "")
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

        public async Task UpsertAndStartTriggerAsync(string name, TriggerResource resource)
        {
            var existingTriggers = await _dataFactoryClient.Triggers.ListByFactoryAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name);
            if (existingTriggers.Any(t => t.Name == name))
            {
                await _dataFactoryClient.Triggers.StopAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name, name);
            }
            
            await _dataFactoryClient.Triggers.CreateOrUpdateAsync(_dataFactoryOptions.ResourceGroup,
                    _dataFactoryOptions.Name, name, resource);

            await _dataFactoryClient.Triggers.StartAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name, name);
        }

        public async Task<IEnumerable<PipelineResource>> GetAllPipelinesAsync()
        {
            var pipelines = await _dataFactoryClient.Pipelines.ListByFactoryAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name);
            return pipelines.ToList();
        }

        public async Task RunPipelineAsync(string name)
        {
            await _dataFactoryClient.Pipelines.CreateRunAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name, name);
        }

        public async Task<IEnumerable<PipelineRun>> GetAllPipelineRunsAsync(int days)
        {
            var response = await _dataFactoryClient.PipelineRuns.QueryByFactoryAsync(_dataFactoryOptions.ResourceGroup, _dataFactoryOptions.Name,
                new RunFilterParameters(DateTime.Now.AddDays(-days), DateTime.Now, 
                orderBy: new List<RunQueryOrderBy>
                {
                    new RunQueryOrderBy { OrderBy = nameof(PipelineRun.RunStart), Order = "DESC" }
                }));

            return response.Value;
        }
    }
}