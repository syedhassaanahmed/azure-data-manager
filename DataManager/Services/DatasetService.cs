using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.IO;
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

        public async Task UpsertAllAsync(IEnumerable<Models.Dataset> datasets)
        {
            foreach (var dataset in datasets)
            {
                Microsoft.Azure.Management.DataFactory.Models.Dataset ds = null;

                switch (dataset.Type)
                {
                    case DatasetType.BlobStorage:
                        await _connectionService.UpsertBlobStorageAsync(dataset.SecretName);

                        var format = GetFormat(Path.GetExtension(dataset.DataPath));
                        var folderPath = Path.GetDirectoryName(dataset.DataPath).Replace("\\", "/");
                        var fileName = Path.GetFileName(dataset.DataPath);

                        ds = new AzureBlobDataset
                        {
                            Description = dataset.Description,
                            LinkedServiceName = new LinkedServiceReference { ReferenceName = dataset.SecretName },
                            FolderPath = folderPath,
                            FileName = fileName,
                            Format = format
                        };
                        break;
                    case DatasetType.SqlServer:
                        await _connectionService.UpsertSqlServerAsync(dataset.SecretName);

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

        private static DatasetStorageFormat GetFormat(string extension)
        {
            var additionalProperties = new Dictionary<string, object>();

            switch (extension)
            {
                case ".json":
                    additionalProperties.Add("type", "JsonFormat");
                    additionalProperties.Add("filePattern", "setOfObjects");
                    break;
                case ".csv":
                    additionalProperties.Add("type", "TextFormat");
                    additionalProperties.Add("firstRowAsHeader", true);
                    additionalProperties.Add("columnDelimiter", ",");
                    break;
                case ".parquet":
                    additionalProperties.Add("type", "ParquetFormat");
                    break;
            }

            return new DatasetStorageFormat(additionalProperties);
        }
    }
}
