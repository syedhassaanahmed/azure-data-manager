using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<(IDictionary<string, ParameterSpecification>, List<(string name, TriggerResource resource)>)> 
            UpsertAllAsync(string pipelineName, IEnumerable<Models.Dataset> datasets)
        {
            var pipelineParameters = new ConcurrentDictionary<string, ParameterSpecification>();
            var triggers = new ConcurrentBag<(string name, TriggerResource resource)>();

            var tasks = datasets.Select(d => UpsertAsync(pipelineName, pipelineParameters, triggers, d));
            await Task.WhenAll(tasks);

            return (pipelineParameters, triggers.ToList());
        }

        private async Task UpsertAsync(string pipelineName, ConcurrentDictionary<string, ParameterSpecification> pipelineParameters, 
            ConcurrentBag<(string name, TriggerResource resource)> triggers, Models.Dataset dataset)
        {
            Microsoft.Azure.Management.DataFactory.Models.Dataset ds = null;
            Task connectionTask = Task.CompletedTask;

            switch (dataset.Type)
            {
                case DatasetType.BlobStorage:
                    connectionTask = _connectionService.UpsertBlobStorageAsync(dataset.SecretName);

                    if (dataset.IsDynamic)
                    {
                        pipelineParameters.GetOrAdd(dataset.FolderParameter, new ParameterSpecification("String"));
                        pipelineParameters.GetOrAdd(dataset.FileParameter, new ParameterSpecification("String"));

                        var triggerResource = _triggerService.CreateBlobEventTrigger(pipelineName, dataset);
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
                    connectionTask = _connectionService.UpsertSqlServerAsync(dataset.SecretName);

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

            await connectionTask;
            await _dataFactoryService.UpsertAsync(dataset.Id, resource);
        }

        private static DatasetStorageFormat GetFormat(string extension)
        {
            var additionalProperties = new Dictionary<string, object>();

            switch (extension.ToLowerInvariant())
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
