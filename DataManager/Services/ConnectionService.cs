using Microsoft.Azure.Management.DataFactory.Models;
using System.Threading.Tasks;
using DataManager.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace DataManager.Services
{
    public class ConnectionService
    {
        private readonly DataFactoryService _dataFactoryService;
        private readonly KeyVaultOptions _keyVaultOptions;
        private readonly DatabricksOptions _databricksOptions;

        public string DatabricksName => _databricksOptions.KeyVaultSecretName;

        public ConnectionService(DataFactoryService dataFactoryService, IOptions<KeyVaultOptions> keyVaultOptions,
            IOptions<DatabricksOptions> databricksOptions)
        {
            _dataFactoryService = dataFactoryService;
            _keyVaultOptions = keyVaultOptions.Value;
            _databricksOptions = databricksOptions.Value;

            UpsertKeyVaultAsync().Wait();
            UpsertDatabricksAsync().Wait();
        }

        private async Task UpsertKeyVaultAsync()
        {
            var service = new AzureKeyVaultLinkedService()
            {
                BaseUrl = $"https://{_keyVaultOptions.Name}.vault.azure.net/"
            };

            await UpsertAsync(_keyVaultOptions.Name, service);
        }

        private AzureKeyVaultSecretReference GetKeyVaultReference(string secretName)
        {
            return new AzureKeyVaultSecretReference
            {
                SecretName = secretName,
                Store = new LinkedServiceReference
                {
                    ReferenceName = _keyVaultOptions.Name
                }
            };
        }

        private async Task UpsertDatabricksAsync()
        {
            var service = new AzureDatabricksLinkedService()
            {
                Domain = _databricksOptions.Endpoint,
                AccessToken = GetKeyVaultReference(_databricksOptions.KeyVaultSecretName)
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

            await UpsertAsync(DatabricksName, service);
        }

        public async Task UpsertBlobStorageAsync(string secretName)
        {
            var service = new AzureBlobStorageLinkedService()
            {
                ConnectionString = GetKeyVaultReference(secretName)
            };

            await UpsertAsync(secretName, service);
        }

        public async Task UpsertSqlServerAsync(string secretName)
        {
            var service = new AzureSqlDatabaseLinkedService()
            {
                ConnectionString = GetKeyVaultReference(secretName)
            };

            await UpsertAsync(secretName, service);
        }

        private async Task UpsertAsync(string name, LinkedService service)
        {
            var resource = new LinkedServiceResource(service);
            resource.Validate();
            await _dataFactoryService.UpsertAsync(name, resource);
        }
    }
}