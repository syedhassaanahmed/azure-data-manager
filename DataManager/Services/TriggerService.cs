using DataManager.Options;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataManager.Services
{
    public class TriggerService
    {        
        private readonly DataFactoryOptions _dataFactoryOptions;

        public TriggerService(IOptions<DataFactoryOptions> dataFactoryOptions)
        {
            _dataFactoryOptions = dataFactoryOptions.Value;
        }

        public TriggerResource CreateBlobEventTrigger(string pipelineName, Models.Dataset dataset, string folderParameter, string fileParameter)
        {
            var storageAccount = "syahschneider";

            var allFolders = Path.GetDirectoryName(dataset.DataPath).Split("\\").ToList();
            allFolders.Insert(2, "blobs");
            var beginsWith = string.Join("/", allFolders) + "/";

            var trigger = new BlobEventsTrigger
            {
                BlobPathBeginsWith = beginsWith,
                BlobPathEndsWith = Path.GetExtension(dataset.DataPath),
                Events = new List<string> { "Microsoft.Storage.BlobCreated" },
                Pipelines = new List<TriggerPipelineReference>
                {
                    new TriggerPipelineReference(new PipelineReference(pipelineName), new Dictionary<string, object>
                    {
                        { folderParameter, "@triggerBody().folderPath" },
                        { fileParameter, "@triggerBody().fileName" }
                    })
                },
                Scope = $"/subscriptions/{_dataFactoryOptions.SubscriptionId}/resourceGroups/{_dataFactoryOptions.ResourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccount}"
            };

            var resource = new TriggerResource(trigger);
            resource.Validate();
            return resource;
        }
    }
}
