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

        // POST: api/Pipeline/123
        [HttpPost]
        [Route("{name}")]
        public async Task CreateAsync(string name)
        {
            await _pipelineService.UpsertAsync(name);
        }

        // POST: api/Pipeline/run/xyz
        [HttpPost]
        [Route("run/{name}")]
        public async Task RunAsync(string name)
        {
            await _pipelineService.RunAsync(name);
        }

        // GET: api/Pipeline/runs/7
        [HttpGet]
        [Route("runs/{days}")]
        public async Task<IEnumerable<object>> GetAllRunsAsync(int days)
        {
            return await _pipelineService.GetAllRunsAsync(days);
        }
    }
}
