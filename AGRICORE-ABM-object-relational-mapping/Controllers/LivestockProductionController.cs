using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing livestock production.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LivestockProductionController : ControllerBase
    {
        private readonly IRepository<LivestockProduction> _repositoryLivestockProduction;
        private readonly ILogger<LivestockProductionController> _logger;

        public LivestockProductionController(
            IRepository<LivestockProduction> repositoryLivestockProduction,
            ILogger<LivestockProductionController> logger
        )
        {
            _repositoryLivestockProduction = repositoryLivestockProduction;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new livestock production entry.
        /// </summary>
        /// <param name="data">Livestock production data to add.</param>
        /// <returns>Created livestock production entry.</returns>

        [HttpPost("/livestockProduction/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LivestockProduction>> AddLivestockProduction(LivestockProduction data)
        {

            var(success,message) = await _repositoryLivestockProduction.AddAsync(data);
            if (success)
            {
                _logger.LogInformation($"LivestockProduction {data.Id} added");
                return CreatedAtAction(nameof(AddLivestockProduction), new { id = data.Id }, data);
            }
            else
            {
                _logger.LogError("Error while inserting LivestockProduction: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds a range of livestock production entries.
        /// </summary>
        /// <param name="data">List of livestock production data to add.</param>
        /// <returns>Created livestock production entries.</returns>

        [HttpPost("/livestockProduction/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LivestockProduction>> AddLivestockProductionRange(List<LivestockProduction> data)
        {
            var (success, message) = await _repositoryLivestockProduction.AddRangeAsync(data);
            if (success)
            {
                var createdIds = data.Select(x => x.Id).ToList();
                _logger.LogInformation($"LivestockProduction {String.Join(",", createdIds)} added");
                return CreatedAtAction(nameof(AddLivestockProductionRange), new { }, data);
            } 
            else
            {
                _logger.LogError(message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all livestock production entries.
        /// </summary>
        /// <returns>List of all livestock production entries.</returns>

        [HttpGet("/livestockProduction/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<LivestockProduction>>> GetAllLivestockProductions()
        {
            var result = await _repositoryLivestockProduction.GetAllAsync();
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no LivestockProductions in the database");
                return new NoContentResult();
            }
            return Ok(result);
        }

    }
}
