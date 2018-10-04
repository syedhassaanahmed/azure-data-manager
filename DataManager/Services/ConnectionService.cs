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

        public ConnectionService(DataFactoryService dataFactoryService, IOptions<KeyVaultOptions> keyVaultOptions,
            IOptions<DatabricksOptions> databricksOptions)
        {
            _dataFactoryService = dataFactoryService;
            _keyVaultOptions = keyVaultOptions.Value;
            _databricksOptions = databricksOptions.Value;

            CreateKeyVaultAsync().Wait();
        }

        private async Task CreateKeyVaultAsync()
        {
            var service = new AzureKeyVaultLinkedService()
            {
                BaseUrl = $"https://{_keyVaultOptions.Name}.vault.azure.net/"
            };

            await UpsertAsync(_keyVaultOptions.Name, service);
        }

        private AzureKeyVaultSecretReference GetKeyVaultReference(string name)
        {
            return new AzureKeyVaultSecretReference
            {
                SecretName = name,
                Store = new LinkedServiceReference
                {
                    ReferenceName = _keyVaultOptions.Name
                }
            };
        }

        public async Task CreateBlobStorageAsync(string name)
        {
            var service = new AzureBlobStorageLinkedService()
            {
                ConnectionString = GetKeyVaultReference(name)
            };

            await UpsertAsync(name, service);
        }

        public async Task CreateSqlServerAsync(string name)
        {
            var service = new AzureSqlDatabaseLinkedService()
            {
                ConnectionString = GetKeyVaultReference(name)
            };

            await UpsertAsync(name, service);
        }

        public async Task CreateDatabricksAsync(string name, string clusterName = "")
        {
            var pythonVersion = $"python{_databricksOptions.PythonVersion}";

            var service = new AzureDatabricksLinkedService()
            {
                Domain = _databricksOptions.Endpoint,
                AccessToken = GetKeyVaultReference(_databricksOptions.KeyVaultSecretName),
                NewClusterNodeType = _databricksOptions.NodeType,
                NewClusterNumOfWorker = _databricksOptions.NumberOfWorkers,
                NewClusterVersion = _databricksOptions.RuntimeVersion,
                NewClusterSparkEnvVars = new Dictionary<string, object>
                {
                    { "PYSPARK_PYTHON", $"/databricks/{pythonVersion}/bin/{pythonVersion}" }
                }
            };

            await UpsertAsync(name, service);
        }

        private async Task UpsertAsync(string name, LinkedService service)
        {
            var resource = new LinkedServiceResource(service);
            resource.Validate();
            await _dataFactoryService.UpsertAsync(name, resource);
        }
    }
}