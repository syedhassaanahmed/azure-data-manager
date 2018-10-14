using System.Collections.Generic;
using System.Threading.Tasks;
using DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataManager.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PipelineController : ControllerBase
    {
        private readonly PipelineService _pipelineService;

        public PipelineController(PipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        // GET: api/Pipeline
        [HttpGet]
        public async Task<IEnumerable<string>> GetAllAsync()
        {
            return await _pipelineService.GetAllAsync();
        }

        // POST: api/Pipeline
        [HttpPost]
        [Route("{name}")]
        public async Task CreateAsync(string name)
        {
            await _pipelineService.UpsertAsync(name);
        }

        // POST: api/Pipeline/run
        [HttpPost]
        [Route("run/{name}")]
        public async Task RunAsync(string name)
        {
            await Task.Delay(5000);
        }
    }
}
