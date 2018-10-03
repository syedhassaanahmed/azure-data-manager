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
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Pipeline/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Pipeline
        [HttpPost]
        public async Task Create([FromBody] string name)
        {
            await _pipelineService.CreateAsync(name);
        }

        // PUT: api/Pipeline/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
