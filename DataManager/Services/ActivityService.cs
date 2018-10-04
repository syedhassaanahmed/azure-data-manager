using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class ActivityService
    {
        private readonly ConnectionService _connectionService;

        public ActivityService(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<ExecutionActivity> CreateDatabricksAsync(string name, string path, Dictionary<string, object> parameters)
        {
            var databricksName = await _connectionService.UpsertDatabricksAsync();

            return new DatabricksNotebookActivity
            {
                Name = name,
                LinkedServiceName = new LinkedServiceReference { ReferenceName = databricksName },
                NotebookPath = path,
                BaseParameters = parameters
            };
        }

        public ExecutionActivity CreateCopy(string name, string from, string to)
        {
            return new CopyActivity
            {
                Name = name,
                Inputs = new List<DatasetReference> { new DatasetReference { ReferenceName = from } },
                Outputs = new List<DatasetReference> { new DatasetReference { ReferenceName = to } },
                Source = new BlobSource { },
                Sink = new BlobSink { }
            };
        }
    }
}
