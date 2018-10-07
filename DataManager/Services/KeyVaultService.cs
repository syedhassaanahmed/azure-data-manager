using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class KeyVaultService
    {
        private readonly DataFactoryService _dataFactoryService;
        private readonly KeyVaultClient _keyVaultClient;

        public KeyVaultService(DataFactoryService dataFactoryService)
        {
            _dataFactoryService = dataFactoryService;
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(_dataFactoryService.GetAuthenticationToken));
        }        

        public async Task GetStorageAccountResourceIdAsync(string secretName)
        {
            var sec = await _keyVaultClient.GetSecretAsync(secretName);
        }
    }
}
