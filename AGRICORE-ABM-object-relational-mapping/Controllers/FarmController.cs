using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing farms related operations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FarmController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly ILogger<FarmController> _logger;

        public FarmController(
            IRepository<Population> repositoryPopulation,
            IRepository<Farm> repositoryFarm,
            ILogger<FarmController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryFarm = repositoryFarm;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new farm to a specified population.
        /// </summary>
        /// <param name="populationId">The ID of the population to add the farm to.</param>
        /// <param name="farm">The farm object to add.</param>
        /// <returns>Returns the added farm on success.</returns>

        [HttpPost("/population/{populationId}/farms/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Farm>> AddFarm(long populationId, Farm farm)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");
                return BadRequest(ModelState);
            }
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farm.FarmCode && f.PopulationId == populationId) != null;

            if (existingFarm)
            {
                error = "This farm already exists";
                _logger.LogError(error);
                return StatusCode(400, error);
            }

            farm.PopulationId = populationId;

            var(success, message) = await _repositoryFarm.AddAsync(farm);
            if (success)
            {
                _logger.LogInformation($"Farm {farm.Id} added");
                return CreatedAtAction(nameof(AddFarm), new { id = farm.Id }, farm);
            }
            else
            {
                _logger.LogError("Error while inserting Farm: "+message);
                return BadRequest(message);
            }


        }

        /// <summary>
        /// Adds a range of farms to a specified population.
        /// </summary>
        /// <param name="populationId">The ID of the population to add the farms to.</param>
        /// <param name="farms">The list of farms to add.</param>
        /// <returns>Returns the IDs of the created farms on success.</returns>

        [HttpPost("/population/{populationId}/farms/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Farm>> AddFarmRange(long populationId, List<Farm> farms)
        {
            //checks if the model in the request is correct
            if (!ModelState.IsValid)
            {
                _logger.LogError(ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            //check if there is any duplicated farmCode value
            var duplicateFarmCodes = farms
                .GroupBy(farm => farm.FarmCode)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();
            string error = string.Empty;
            if (duplicateFarmCodes.Any())
            {
                error = $"Duplicate farm codes found: {string.Join(", ", duplicateFarmCodes)}";
                _logger.LogError(error);
                return BadRequest(error);
            }
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);

            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            //checks if there is any previously contained farmCode value
            //var existingFarms = await _repositoryFarm.GetAllAsync(f => farms.Where(q => q.PopulationId == populationId).Select(farm => farm.FarmCode ).Contains(f.FarmCode));

            foreach (Farm farm in farms)
            {
                var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farm.FarmCode);
                if (existingFarm != null)
                {
                    error = "One or more farms already exist.";
                    _logger.LogError(error);
                    return StatusCode(400, error);
                }
            }

            farms.ForEach(f => f.PopulationId = populationId);

            var(success,message) = await _repositoryFarm.AddRangeAsync(farms);
            if(success)
            {
                var createdFarmIds = farms.Select(farm => farm.Id).ToList();
                _logger.LogInformation($"Farms {String.Join(",", createdFarmIds)} added");
                var result = new { ids = createdFarmIds };

                return CreatedAtAction(nameof(AddFarmRange), new { }, result);
            }
            else
            {
                _logger.LogError("Error while inserting Farms: " +message);
                return BadRequest(message);
            }

            
        }

        /// <summary>
        /// Retrieves all farms associated with a specific population.
        /// </summary>
        /// <param name="populationId">The ID of the population to retrieve farms for.</param>
        /// <returns>Returns a list of farms associated with the specified population on success.</returns>

        [HttpGet("/population/{populationId}/farms/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<Farm>>> GetAllFarms(long populationId)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Farms), asNoTracking: true);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            if (existingPopulation.Farms == null || existingPopulation.Farms.Count == 0)
            {
                error = "No entities found.";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(existingPopulation.Farms);
        }

        /// <summary>
        /// Retrieves a specific farm by its ID.
        /// </summary>
        /// <param name="farmId">The ID of the farm to retrieve.</param>
        /// <returns>Returns the farm with the specified ID on success.</returns>

        [HttpGet("/farms/{farmId}/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<Farm>>> GetFarm(long farmId)
        {
            var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.Id == farmId);
            string error = string.Empty;
            if (existingFarm == null)
            {
                error = "This farm does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(existingFarm);
        }

    }
}
