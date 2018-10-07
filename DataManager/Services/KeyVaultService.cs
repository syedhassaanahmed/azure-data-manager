using DataManager.Options;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class KeyVaultService
    {
        private readonly DataFactoryService _dataFactoryService;
        private readonly KeyVaultOptions _keyVaultOptions;
        private readonly KeyVaultClient _keyVaultClient;

        private string BaseUrl => $"https://{_keyVaultOptions.Name}.vault.azure.net/";

        public KeyVaultService(DataFactoryService dataFactoryService, IOptions<KeyVaultOptions> keyVaultOptions)
        {
            _dataFactoryService = dataFactoryService;
            _keyVaultOptions = keyVaultOptions.Value;
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(_dataFactoryService.GetAuthenticationToken));

            UpsertKeyVaultAsync().Wait();
        }

        private async Task UpsertKeyVaultAsync()
        {
            var service = new AzureKeyVaultLinkedService()
            {
                BaseUrl = BaseUrl
            };

            var resource = new LinkedServiceResource(service);
            resource.Validate();
            await _dataFactoryService.UpsertAsync(_keyVaultOptions.Name, resource);
        }

        public AzureKeyVaultSecretReference GetKeyVaultReference(string name)
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

        public async Task<string> GetStorageAccountResourceIdAsync(string secretName)
        {
            var secret = await _keyVaultClient.GetSecretAsync(BaseUrl, secretName);
            var storageAccount = secret.Value.Split(";").First(x => x.StartsWith("AccountName=")).Split("=")[1];
            return $"/subscriptions/{_dataFactoryService.SubscriptionId}/resourceGroups/{_dataFactoryService.ResourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccount}";
        }
    }
}
