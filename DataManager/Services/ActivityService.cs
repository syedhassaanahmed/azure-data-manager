using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;

namespace DataManager.Services
{
    internal class ActivityService
    {
        private ActivityService()
        {
        }

        internal static ActivityService Current = new ActivityService();

        //internal ExecutionActivity CreateDatabricks(string name, string path, string type, Dictionary<string, object> parameters)
        //{
        //    var service = ConnectionService.Current.GetByType(type);

        //    return new DatabricksNotebookActivity()
        //    {
        //        Name = name,
        //        LinkedServiceName = new LinkedServiceReference() { ReferenceName = service },
        //        NotebookPath = path,
        //        BaseParameters = parameters
        //    };
        //}

        //internal ExecutionActivity CreateAdfCopy(string name, string from, string to)
        //{
        //    return new CopyActivity()
        //    {
        //        Name = name,
        //        Inputs = new List<DatasetReference> { new DatasetReference() { ReferenceName = from } },
        //        Outputs = new List<DatasetReference> { new DatasetReference() { ReferenceName = to } },
        //        Source = new BlobSource { },
        //        Sink = new BlobSink { }
        //    };
        //}
    }
}
