using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataManager.Services
{
    public class PipelineService
    {
        private readonly CosmosDbService _cosmosDbService;
        private readonly DatasetService _datasetService;

        private readonly Dictionary<string, string> _pipelineNameRunIdList = new Dictionary<string, string>();

        public PipelineService(CosmosDbService cosmosDbService, DatasetService datasetService)
        {
            _cosmosDbService = cosmosDbService;
            _datasetService = datasetService;
        }

        public async Task CreateAsync(string name)
        {
            var datasets = await _cosmosDbService.ReadAllAsync<Models.Dataset>("dataset");
            await _datasetService.CreateAllAsync(datasets);

            /*var jobList = _cosmosDbService.ReadAll<Job>("job");

            var pipeline = new PipelineResource
            {
                Activities = new List<Activity>()
            };

            foreach (var job in jobList)
            {
                ExecutionActivity activity = null;

                switch (job.Specification.Type)
                {
                    case "Databricks":
                        {
                            var parameters = new Dictionary<string, object>();
                            foreach (var p in job.Specification.NotebookParameters)
                            {
                                var (Path, Name) = DatasetService.Current.GetById(p.DatasetId);

                                if (!string.IsNullOrWhiteSpace(Path))
                                {
                                    parameters.Add(p.Name, Path);
                                }
                            }

                            activity = ActivityService.Current.CreateDatabricks(job.Id, job.Specification.NotebookPath, 
                                job.Specification.Type, parameters);
                        }
                        break;
                    case "ADFCopy":
                        {
                            var from = DatasetService.Current.GetById(job.From.FirstOrDefault()).Name;
                            var to = DatasetService.Current.GetById(job.To.FirstOrDefault()).Name;

                            activity = ActivityService.Current.CreateAdfCopy(job.Id, from, to);
                        }
                        break;
                }

                if (job.DependsOn != null && job.DependsOn.Any())
                {
                    activity.DependsOn = job.DependsOn.Select(x =>
                        new ActivityDependency() { Activity = x, DependencyConditions = new List<string>() { "Succeeded" } })
                        .ToList();
                }

                pipeline.Activities.Add(activity);
            }

            _pipelineNameRunIdList.Add(name, string.Empty);

            DataFactoryService.Current.DataFactoryClient.Pipelines.CreateOrUpdate(Configuration.ResourceGroupName, 
                Configuration.DataFactoryName, name, pipeline);*/
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
