using Microsoft.Azure.Management.DataFactory.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using DataManager.Helpers;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class ConnectionService
    {
        private readonly DataFactoryService _dataFactoryService;

        public ConnectionService(DataFactoryService dataFactoryService)
        {
            _dataFactoryService = dataFactoryService;
        }

        public async Task<string> CreateBlobStorageAsync()
        {
            var name = "BlobStorage";

            var service = new AzureDataLakeStoreLinkedService()
            {
                DataLakeStoreUri = Configuration.DataLakeStorageUri
            };

            await _dataFactoryService.UpsertAsync(name, new LinkedServiceResource(service));

            return name;
        }

        public async Task<string> CreateDatabricksAsync(string clusterName = "")
        {
            var name = "Databricks";

            var httpClient = new HttpClient() { BaseAddress = new Uri($"{Configuration.DatabricksEndpoint}/api/2.0/") };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration.DatabricksAccessToken}");

            var jsonResult = httpClient.GetStringAsync("clusters/list").GetAwaiter().GetResult();
            var clusterList = JsonConvert.DeserializeObject<ClusterListResponse>(jsonResult).clusters;

            var existingClusterId = clusterList.FirstOrDefault().cluster_id;

            if (!string.IsNullOrWhiteSpace(clusterName))
            {
                existingClusterId = clusterList.FirstOrDefault(e => e.cluster_name == clusterName)?.cluster_id;
            }

            var service = new AzureDatabricksLinkedService()
            {
                Domain = Configuration.DatabricksEndpoint,
                AccessToken = new SecureString(Configuration.DatabricksAccessToken),
                ExistingClusterId = existingClusterId
            };

            await _dataFactoryService.UpsertAsync(name, new LinkedServiceResource(service));

            return name;
        }

        public async Task<string> CreateSqlServerAsync()
        {
            var name = "SQLServer";

            

            return name;
        }

        public async Task CreateAllAsync()
        {
            await Task.WhenAll(
                CreateDatabricksAsync(),
                CreateBlobStorageAsync(),
                CreateSqlServerAsync()
            );
        }
    }
}