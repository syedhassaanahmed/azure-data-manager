using DataManager.Options;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace DataManager.Services
{
    public class TriggerService
    {
        private readonly DataFactoryOptions _dataFactoryOptions;
        private readonly StorageAccountOptions _storageAccountOptions;

        public TriggerService(IOptions<DataFactoryOptions> dataFactoryOptions, IOptions<StorageAccountOptions> storageAccountOptions)
        {
            _dataFactoryOptions = dataFactoryOptions.Value;
            _storageAccountOptions = storageAccountOptions.Value;
        }

        public TriggerResource CreateBlobEventTrigger(string pipelineName, Models.Dataset dataset)
        {
            var storageAccountResourceId = $"/subscriptions/{_dataFactoryOptions.SubscriptionId}/resourceGroups/{_dataFactoryOptions.ResourceGroup}/providers/Microsoft.Storage/storageAccounts/{_storageAccountOptions.Name}";

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