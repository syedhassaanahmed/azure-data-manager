using DataManager.Models;
using Microsoft.Azure.Management.DataFactory.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class PipelineService
    {
        private readonly CosmosDbService _cosmosDbService;
        private readonly DatasetService _datasetService;
        private readonly DataFactoryService _dataFactoryService;
        private readonly ActivityService _activityService;
        private readonly TriggerService _triggerService;

        public PipelineService(CosmosDbService cosmosDbService, DatasetService datasetService, 
            DataFactoryService dataFactoryService, ActivityService activityService, TriggerService triggerService)
        {
            _cosmosDbService = cosmosDbService;
            _datasetService = datasetService;
            _dataFactoryService = dataFactoryService;
            _activityService = activityService;
            _triggerService = triggerService;
        }

        public async Task UpsertAsync(string name)
        {
            var allDatasets = await _cosmosDbService.ReadAllAsync<Models.Dataset>("dataset");
            await _datasetService.UpsertAllAsync(allDatasets);

            var activeJobs = (await _cosmosDbService.ReadAllAsync<Job>("job")).Where(x => x.IsActive);

            var pipeline = new PipelineResource
            {
                Activities = new List<Activity>(),
                Parameters = new Dictionary<string, ParameterSpecification>()
            };

            var triggers = new List<(string name, TriggerResource resource)>();

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
                                var (dataPath, folderParameter, fileParameter) = GetDataPath(dataset, allDatasets);
                                notebookParameters.Add(p.Name, dataPath);

                                if (dataset.IsDynamic)
                                {
                                    pipeline.Parameters.Add(folderParameter, new ParameterSpecification("String"));
                                    pipeline.Parameters.Add(fileParameter, new ParameterSpecification("String"));

                                    var triggerResource = _triggerService.CreateBlobEventTrigger(name, dataset, folderParameter, fileParameter);
                                    triggers.Add(($"blobTrigger_{dataset.Id}", triggerResource));                
                                }
                            }

                            activity = await _activityService.CreateDatabricksAsync(job.Id, job.Specification.NotebookPath, notebookParameters);
                        }
                        break;
                    case JobType.Copy:
                        {
                            activity = _activityService.CreateCopy(job.Id, job.From.First(), job.To.First());
                        }
                        break;
                }

                activity.DependsOn = GetDependencies(job, activeJobs);
                pipeline.Activities.Add(activity);
            }

            pipeline.Validate();
            await _dataFactoryService.UpsertAsync(name, pipeline);

            foreach(var trigger in triggers)
            {
                await _dataFactoryService.UpsertAsync(trigger.name, trigger.resource);
            }
        }

        private (string dataPath, string folderParameter, string fileParameter) GetDataPath(
            Models.Dataset dataset, IEnumerable<Models.Dataset> allDatasets)
        {
            if(!dataset.IsDynamic)
                return (dataset.DataPath, "", "");

            var folderParameter = $"folderPath_{dataset.Id}";
            var fileParameter = $"fileName_{dataset.Id}";
            var dataPath = $"@if(equals(pipeline().TriggerType, 'BlobEventsTrigger'), concat('/', pipeline().parameters.{folderParameter}, '/', pipeline().parameters.{fileParameter}), '{dataset.DataPath}')";

            return (dataPath, folderParameter, fileParameter);
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

        /*public void RunAll()
        {
            foreach (var item in _pipelineNameRunIdList)
            {
                var resultBody = DataFactoryService.Current.DataFactoryClient.Pipelines.CreateRunWithHttpMessagesAsync(
                    Configuration.ResourceGroupName, Configuration.DataFactoryName, item.Key).Result.Body;
            }
        }

        public void RunOne(string name)
        {
            var resultBody = DataFactoryService.Current.DataFactoryClient.Pipelines.CreateRunWithHttpMessagesAsync(
                Configuration.ResourceGroupName, Configuration.DataFactoryName, name).Result.Body;
        }

        public IEnumerable<(string Name, string Status)> GetPipelineStatuses()
        {
            foreach (var item in _pipelineNameRunIdList)
            {
                var run = DataFactoryService.Current.DataFactoryClient.PipelineRuns.Get(Configuration.ResourceGroupName, 
                    Configuration.DataFactoryName, item.Value);

                yield return (item.Key, run.Status); // InProgress , Succeeded , Failed
            }
        }

        public void ClearSuccessfullyFinishedPipelines()
        {
            foreach (var item in _pipelineNameRunIdList)
            {
                var runs = DataFactoryService.Current.DataFactoryClient.ActivityRuns.QueryByPipelineRun(
                    Configuration.ResourceGroupName, Configuration.DataFactoryName, item.Value, new RunFilterParameters()).Value;

                foreach (var pipeline in runs.Where(e => e.Status == "Succeeded"))
                {
                    DataFactoryService.Current.DataFactoryClient.Pipelines.Delete(Configuration.ResourceGroupName, 
                        Configuration.DataFactoryName, pipeline.PipelineName);
                }
            }
        }

        public void RemovePipeline(string name)
        {
            DataFactoryService.Current.DataFactoryClient.Pipelines.Delete(Configuration.ResourceGroupName, 
                Configuration.DataFactoryName, name);
        }

        public void RaiseAlertForFailedPipelines()
        {
            foreach (var item in _pipelineNameRunIdList)
            {
                var runs = DataFactoryService.Current.DataFactoryClient.ActivityRuns.QueryByPipelineRun(
                    Configuration.ResourceGroupName, Configuration.DataFactoryName, item.Value, new RunFilterParameters()).Value;

                foreach (var pipeline in runs.Where(e => e.Status == "Failed"))
                {
                    // RAISE ALERT
                }
            }
        }*/
    }
}
