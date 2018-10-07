using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IList<Activity>> CreateAllActivitiesAsync(IEnumerable<Job> activeJobs, IEnumerable<Models.Dataset> allDatasets)
        {
            var activities = new List<Activity>();

            foreach (var job in activeJobs)
            {
                ExecutionActivity activity = null;

                switch (job.Specification.Type)
                {
                    case JobType.Databricks:
                        {
                            var notebookParameters = new Dictionary<string, object>();
                            foreach (var p in job.Specification.NotebookParameters)
                            {
                                var dataset = allDatasets.First(x => x.Id == p.DatasetId);
                                notebookParameters.Add(p.Name, dataset.DataPathExpression);
                            }

                            var databricksName = await _connectionService.UpsertDatabricksAsync();

                            activity = new DatabricksNotebookActivity
                            {
                                Name = job.Id,
                                LinkedServiceName = new LinkedServiceReference { ReferenceName = databricksName },
                                NotebookPath = job.Specification.NotebookPath,
                                BaseParameters = notebookParameters
                            };
                        }
                        break;
                    case JobType.Copy:
                        {
                            activity = new CopyActivity
                            {
                                Name = job.Id,
                                Inputs = new List<DatasetReference> { new DatasetReference { ReferenceName = job.From.First() } },
                                Outputs = new List<DatasetReference> { new DatasetReference { ReferenceName = job.To.First() } },
                                Source = new BlobSource { },
                                Sink = new BlobSink { }
                            };
                        }
                        break;
                }

                activity.DependsOn = GetDependencies(job, activeJobs);
                activity.Validate();
                activities.Add(activity);
            }
            
            return activities;
        }

        private static IList<ActivityDependency> GetDependencies(Job currentJob, IEnumerable<Job> allJobs)
        {
            var dependentJobs = from j in allJobs
                                where j.Id != currentJob.Id && currentJob.From.Any(f => j.To.Contains(f))
                                select j.Id;

            return dependentJobs.Select(j =>
                new ActivityDependency { Activity = j, DependencyConditions = new List<string> { "Succeeded" } })
                .ToList();
        }
    }
}