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
            var allDatasets = await _cosmosDbService.ReadAllAsync<Models.Dataset>("dataset");
            var (parameters, triggers) = await _datasetService.UpsertAllAsync(name, allDatasets);

            var activeJobs = (await _cosmosDbService.ReadAllAsync<Job>("job")).Where(j => j.IsActive);
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
            return pipelines.Select(x => x.Name);
        }
    }
}