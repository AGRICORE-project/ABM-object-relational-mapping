using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing years associated with populations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class YearController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Year> _repositoryYear;
        private readonly ILogger<YearController> _logger;

        public YearController(
            IRepository<Population> repositoryPopulation,
            IRepository<Year> repositoryYear,
            ILogger<YearController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryYear = repositoryYear;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new year to a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to add the year to.</param>
        /// <param name="year">Year entity to add.</param>
        /// <returns>The added year.</returns>

        [HttpPost("/population/{populationId}/years/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Year>> AddYear(long populationId, Year year)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Years));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }
            
            if (existingPopulation.Years != null && existingPopulation.Years.Any(y => y.YearNumber == year.YearNumber))
            {
                error = "This year already exists";
                _logger.LogError(error);
                return BadRequest(error);
            }

            year.PopulationId = populationId;

            var(success,message) = await _repositoryYear.AddAsync(year);
            if(!success)
            {
                error = $"Error while inserting year {year} "+message;
                _logger.LogError(error);
                return BadRequest(error);
            }

            _logger.LogInformation($"Year {year.Id} added");
            return CreatedAtAction(nameof(AddYear), new { id = year.Id }, year);
        }

        /// <summary>
        /// Retrieves all years associated with a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to retrieve years for.</param>
        /// <returns>List of years associated with the population.</returns>

        [HttpGet("/population/{populationId}/years/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<Year>>> GetAllYears(long populationId)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Years));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation (error);
                return StatusCode(409, error);
            }

            if (existingPopulation.Years == null || existingPopulation.Years.Count == 0)
            {
                error = "No entities found.";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(existingPopulation.Years);
        }

        /// <summary>
        /// Retrieves a specific year by its ID.
        /// </summary>
        /// <param name="yearId">ID of the year to retrieve.</param>
        /// <returns>The year entity.</returns>

        [HttpGet("/years/{yearId}/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<Year>>> GetYear(long yearId)
        {
            var existingYear = await _repositoryYear.GetSingleOrDefaultAsync(y => y.Id == yearId);

            if (existingYear == null)
            {
                _logger.LogInformation("This year does not exist");
                return StatusCode(404, "This year does not exist");
            }

            return Ok(existingYear);
        }

    }
}
