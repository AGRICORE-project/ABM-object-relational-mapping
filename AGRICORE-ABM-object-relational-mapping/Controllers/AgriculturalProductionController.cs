using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing agricultural production entities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AgriculturalProductionController : ControllerBase
    {
        private readonly IRepository<AgriculturalProduction> _repositoryAgriculturalProduction;
        private readonly ILogger<AgriculturalProductionController> _logger;

        public AgriculturalProductionController(
            IRepository<AgriculturalProduction> repositoryAgriculturalProduction,
            ILogger<AgriculturalProductionController> logger
        )
        {
            _repositoryAgriculturalProduction = repositoryAgriculturalProduction;
            _logger = logger;
        }

        /// <summary>
        /// Adds a single agricultural production entity to the database.
        /// </summary>
        /// <param name="data">The agricultural production object to add.</param>
        /// <returns>Returns the added agricultural production on success.</returns>
        [HttpPost("/agriculturalProduction/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AgriculturalProduction>> AddAgriculturalProduction(AgriculturalProduction data)
        {
            var (success, errorMessage) = await _repositoryAgriculturalProduction.AddAsync(data);

            if (success)
            {
                _logger.LogInformation($"AgriculturalProduction {data.Id} added");
                return CreatedAtAction(nameof(AddAgriculturalProduction), new { id = data.Id }, data);
            }
            else
            {
                _logger.LogError("Error while adding AgriculturalProduction: "+errorMessage);
                return BadRequest(errorMessage); // Returning the error message in the response.
            }
        }

        /// <summary>
        /// Adds a range of agricultural production entities to the database.
        /// </summary>
        /// <param name="data">List of agricultural production objects to add.</param>
        /// <returns>Returns the added agricultural productions on success.</returns>
        [HttpPost("/agriculturalProduction/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AgriculturalProduction>> AddAgriculturalProductionRange(List<AgriculturalProduction> data)
        {
            var (success, errorMessage) = await _repositoryAgriculturalProduction.AddRangeAsync(data);
            if (success)
            {
                var createdAgriculturalPorductionsIds = data.Select(x => x.Id).ToList();
                _logger.LogInformation($"AgriculturalProduction {String.Join(",",createdAgriculturalPorductionsIds)} added");
                return CreatedAtAction(nameof(AddAgriculturalProductionRange), new { }, data);
            }
            else{ 
                _logger.LogError("Error while adding AgriculturalProductions: " + errorMessage);
                return BadRequest(errorMessage); ;
            }
        }

        /// <summary>
        /// Retrieves all agricultural production entities from the database.
        /// </summary>
        /// <returns>Returns a list of agricultural productions on success.</returns>
        [HttpGet("/agriculturalProduction/get")]
        [HttpGet("/agriculturalProduction/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<AgriculturalProduction>>> GetAllAgriculturalProductions()
        {
            var result = await _repositoryAgriculturalProduction.GetAllAsync(asNoTracking: true);
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no AgriculturalProductions in the database");
                return new NoContentResult();
            }
            return Ok(result);
        }

    }
}
