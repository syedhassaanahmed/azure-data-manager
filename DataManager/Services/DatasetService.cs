using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class DatasetService
    {
        private readonly DataFactoryService _dataFactoryService;
        private readonly ConnectionService _connectionService;

        public DatasetService(DataFactoryService dataFactoryService, ConnectionService connectionService)
        {
            _dataFactoryService = dataFactoryService;
            _connectionService = connectionService;
        }

        public async Task CreateAllAsync(IEnumerable<Models.Dataset> datasets)
        {
            foreach (var dataset in datasets)
            {
                Microsoft.Azure.Management.DataFactory.Models.Dataset ds = null;

                switch (dataset.Type)
                {
                    case DatasetType.BlobStorage:
                        await _connectionService.CreateBlobStorageAsync(dataset.SecretName);

                        var format = GetFormat(dataset.DataPath);

                        ds = new AzureBlobDataset
                        {
                            Description = dataset.Description,
                            LinkedServiceName = new LinkedServiceReference { ReferenceName = dataset.SecretName },
                            FolderPath = dataset.DataPath,
                            Format = format
                        };
                        break;
                    case DatasetType.SqlServer:
                        await _connectionService.CreateSqlServerAsync(dataset.SecretName);

                        ds = new AzureSqlTableDataset
                        {
                            LinkedServiceName = new LinkedServiceReference { ReferenceName = dataset.SecretName },
                            TableName = dataset.DataPath
                        };
                        break;
                }

                ds.Annotations = dataset.Tags;
                var resource = new DatasetResource(ds);
                resource.Validate();
                await _dataFactoryService.UpsertAsync(dataset.Id, resource);
            }
        }

        private static DatasetStorageFormat GetFormat(string dataPath)
        {
            DatasetStorageFormat format = null;

            if (dataPath.Trim().EndsWith(".json"))
            {
                var additionalProperties = new Dictionary<string, object>
                {
                    { "type", "JsonFormat" },
                    { "filePattern", "setOfObjects" }
                };

                format = new DatasetStorageFormat(additionalProperties);
            }

            return format;
        }

        //public (string Path, string Name) GetById(string id)
        //{
        //    if (_list.ContainsKey(id))
        //    {
        //        return _list[id];
        //    }

        //    return ("", "");
        //}
    }
}
