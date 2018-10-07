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
        private readonly TriggerService _triggerService;

        public DatasetService(DataFactoryService dataFactoryService, ConnectionService connectionService, TriggerService triggerService)
        {
            _dataFactoryService = dataFactoryService;
            _connectionService = connectionService;
            _triggerService = triggerService;
        }

        public async Task<(Dictionary<string, ParameterSpecification>, List<(string name, TriggerResource resource)>)> 
            UpsertAllAsync(string pipelineName, IEnumerable<Models.Dataset> datasets)
        {
            var pipelineParameters = new Dictionary<string, ParameterSpecification>();
            var triggers = new List<(string name, TriggerResource resource)>();

            foreach (var dataset in datasets)
            {
                Microsoft.Azure.Management.DataFactory.Models.Dataset ds = null;

                switch (dataset.Type)
                {
                    case DatasetType.BlobStorage:
                        await _connectionService.UpsertBlobStorageAsync(dataset.SecretName);

                        if (dataset.IsDynamic)
                        {
                            pipelineParameters.Add(dataset.FolderParameter, new ParameterSpecification("String"));
                            pipelineParameters.Add(dataset.FileParameter, new ParameterSpecification("String"));

                            var triggerResource = await _triggerService.CreateBlobEventTriggerAsync(pipelineName, dataset);
                            triggers.Add(($"blobTrigger_{dataset.Id}", triggerResource));
                        }

                        ds = new AzureBlobDataset
                        {
                            Description = dataset.Description,
                            LinkedServiceName = new LinkedServiceReference { ReferenceName = dataset.SecretName },
                            FolderPath = dataset.FolderPath.TrimStart('/'),
                            FileName = dataset.FileName,
                            Format = GetFormat(dataset.FileExtension)
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

            return (pipelineParameters, triggers);
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
