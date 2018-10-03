using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class DatasetService
    {
        private readonly DataFactoryService _dataFactoryService;

        private readonly IDictionary<string, (string Path, string Name)> _list = 
            new Dictionary<string, (string Path, string Name)>();

        public DatasetService(DataFactoryService dataFactoryService)
        {
            _dataFactoryService = dataFactoryService;
        }

        public async Task CreateAllAsync(IEnumerable<Models.Dataset> datasetList)
        {
            foreach (var dataset in datasetList)
            {
                var serviceName = string.Empty;

                Microsoft.Azure.Management.DataFactory.Models.Dataset ds = null;

                switch (dataset.Type)
                {
                    case DatasetType.BlobStorage:
                        var format = GetFormat(dataset.DataPath);

                        ds = new AzureDataLakeStoreDataset()
                        {
                            Description = dataset.Description,
                            LinkedServiceName = new LinkedServiceReference() { ReferenceName = serviceName },
                            FolderPath = dataset.DataPath,
                            Format = format
                        };
                        break;
                    case DatasetType.SqlServer:
                        ds = new MongoDbCollectionDataset()
                        {
                            LinkedServiceName = new LinkedServiceReference() { ReferenceName = serviceName },
                            CollectionName = dataset.DataPath
                        };
                        break;
                }

                _list.Add(dataset.Id, (dataset.DataPath, dataset.Id));

                await _dataFactoryService.UpsertAsync(dataset.Id, new DatasetResource(ds));
            }
        }

        private static DatasetStorageFormat GetFormat(string dataPath)
        {
            DatasetStorageFormat format = null;

            if (dataPath.Trim().EndsWith(".json"))
            {
                var additionalProperties = new Dictionary<string, object>() {
                                { "type", "JsonFormat" },
                                { "filePattern", "setOfObjects" }
                            };

                format = new DatasetStorageFormat(additionalProperties);
            }

            return format;
        }

        public (string Path, string Name) GetById(string id)
        {
            if (_list.ContainsKey(id))
            {
                return _list[id];
            }

            return ("", "");
        }
    }
}
