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

        public PipelineService(CosmosDbService cosmosDbService, DatasetService datasetService, 
            DataFactoryService dataFactoryService, ActivityService activityService)
        {
            _cosmosDbService = cosmosDbService;
            _datasetService = datasetService;
            _dataFactoryService = dataFactoryService;
            _activityService = activityService;            
        }

        public async Task UpsertAsync(string name)
        {
            var allDatasets = await _cosmosDbService.GetAllAsync<Models.Dataset>("dataset");
            var (parameters, triggers) = await _datasetService.UpsertAllAsync(name, allDatasets);

            var activeJobs = (await _cosmosDbService.GetAllAsync<Job>("job")).Where(j => j.IsActive);
            var activities = _activityService.CreateAll(activeJobs, allDatasets);

            var pipeline = new PipelineResource
            {
                Activities = activities,
                Parameters = parameters
            };

            pipeline.Validate();
            await _dataFactoryService.UpsertAsync(name, pipeline);

            var triggerTasks = triggers.Select(t => _dataFactoryService.UpsertAndStartTriggerAsync(t.name, t.resource));
            await Task.WhenAll(triggerTasks);
        }

        public async Task<IEnumerable<string>> GetAllAsync()
        {
            var pipelines = await _dataFactoryService.GetAllPipelinesAsync();
            return pipelines.Select(p => p.Name);
        }

        public async Task RunAsync(string name)
        {
            await _dataFactoryService.RunPipelineAsync(name);
        }

        public async Task<IEnumerable<object>> GetAllRunsAsync(int days)
        {
            var runs = await _dataFactoryService.GetAllPipelineRunsAsync(days);
            return runs.Select(r => new { r.PipelineName, r.RunStart, r.RunEnd, r.Status });
        }
    }
}