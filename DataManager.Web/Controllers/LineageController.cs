using System.Linq;
using System.Threading.Tasks;
using DataManager.Models;
using DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataManager.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineageController : ControllerBase
    {
        private readonly CosmosDbService _cosmosDbService;

        public LineageController(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        // GET: api/Lineage
        [HttpGet]
        public async Task<object> GetAllAsync()
        {
            var allDatasets = await _cosmosDbService.GetAllAsync<Dataset>("dataset");
            var activeJobs = (await _cosmosDbService.GetAllAsync<Job>("job")).Where(j => j.IsActive);

            var result = new
            {
                Datasets = allDatasets.Select(d => new { d.Id, Label = d.Description }),
                Jobs = activeJobs.SelectMany(j => j.From, (j, From) => new { From, j.To, Label = j.Id })
                    .SelectMany(j => j.To, (j, To) => new { j.From, To, j.Label })
            };

            return result;
        }
    }
}
