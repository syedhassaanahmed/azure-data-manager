using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class TriggerService
    {        
        private readonly KeyVaultService _keyVaultService;

        public TriggerService(KeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
        }

        public async Task<TriggerResource> CreateBlobEventTriggerAsync(string pipelineName, Models.Dataset dataset)
        {
            var storageAccountResourceId = await _keyVaultService.GetStorageAccountResourceIdAsync(dataset.SecretName);

            var allFolders = dataset.FolderPath.Split("/").ToList();
            allFolders.Insert(2, "blobs");
            var beginsWith = string.Join("/", allFolders) + "/";

            var trigger = new BlobEventsTrigger
            {
                BlobPathBeginsWith = beginsWith,
                BlobPathEndsWith = dataset.FileExtension,
                Events = new List<string> { "Microsoft.Storage.BlobCreated" },
                Pipelines = new List<TriggerPipelineReference>
                {
                    new TriggerPipelineReference(new PipelineReference(pipelineName), new Dictionary<string, object>
                    {
                        { dataset.FolderParameter, "@triggerBody().folderPath" },
                        { dataset.FileParameter, "@triggerBody().fileName" }
                    })
                },
                Scope = storageAccountResourceId
            };

            var resource = new TriggerResource(trigger);
            resource.Validate();
            return resource;
        }
    }
}
