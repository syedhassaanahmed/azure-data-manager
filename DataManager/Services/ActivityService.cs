using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataManager.Services
{
    public class ActivityService
    {
        private readonly ConnectionService _connectionService;

        public ActivityService(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public IList<Activity> CreateAll(IEnumerable<Job> activeJobs, IEnumerable<Models.Dataset> allDatasets)
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
                            foreach (var parameter in job.Specification.NotebookParameters)
                            {
                                var dataset = allDatasets.First(d => d.Id == parameter.DatasetId);
                                notebookParameters.Add(parameter.Name, dataset.DataPathExpression);
                            }

                            activity = new DatabricksNotebookActivity
                            {
                                Name = job.Id,
                                LinkedServiceName = new LinkedServiceReference { ReferenceName = _connectionService.DatabricksName},
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
                                Inputs = new List<DatasetReference> { new DatasetReference { ReferenceName = job.From.Single() } },
                                Outputs = new List<DatasetReference> { new DatasetReference { ReferenceName = job.To.Single() } },
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