﻿using Microsoft.Azure.Management.DataFactory.Models;
using System.Threading.Tasks;
using DataManager.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace DataManager.Services
{
    public class ConnectionService
    {
        private readonly DataFactoryService _dataFactoryService;
        private readonly KeyVaultService _keyVaultService;
        private readonly DatabricksOptions _databricksOptions;

        public ConnectionService(DataFactoryService dataFactoryService, KeyVaultService keyVaultService,
            IOptions<DatabricksOptions> databricksOptions)
        {
            _dataFactoryService = dataFactoryService;
            _keyVaultService = keyVaultService;
            _databricksOptions = databricksOptions.Value;
        }

        public async Task UpsertBlobStorageAsync(string name)
        {
            var service = new AzureBlobStorageLinkedService()
            {
                ConnectionString = _keyVaultService.GetKeyVaultReference(name)
            };

            await UpsertAsync(name, service);
        }

        public async Task UpsertSqlServerAsync(string name)
        {
            var service = new AzureSqlDatabaseLinkedService()
            {
                ConnectionString = _keyVaultService.GetKeyVaultReference(name)
            };

            await UpsertAsync(name, service);
        }

        public async Task<string> UpsertDatabricksAsync()
        {
            var name = _databricksOptions.KeyVaultSecretName;            

            var service = new AzureDatabricksLinkedService()
            {
                Domain = _databricksOptions.Endpoint,
                AccessToken = _keyVaultService.GetKeyVaultReference(_databricksOptions.KeyVaultSecretName)
            };

            if (!string.IsNullOrWhiteSpace(_databricksOptions.ExistingClusterId))
            {
                service.ExistingClusterId = _databricksOptions.ExistingClusterId;
            }
            else
            {
                var newCluster = _databricksOptions.NewCluster;

                service.NewClusterNodeType = newCluster.NodeType;
                service.NewClusterNumOfWorker = newCluster.NumberOfWorkers;
                service.NewClusterVersion = newCluster.RuntimeVersion;

                var pythonVersion = $"python{newCluster.PythonVersion}";
                service.NewClusterSparkEnvVars = new Dictionary<string, object>
                {
                    { "PYSPARK_PYTHON", $"/databricks/{pythonVersion}/bin/{pythonVersion}" }
                };
            }

            await UpsertAsync(name, service);
            return name;
        }

        private async Task UpsertAsync(string name, LinkedService service)
        {
            var resource = new LinkedServiceResource(service);
            resource.Validate();
            await _dataFactoryService.UpsertAsync(name, resource);
        }
    }
}