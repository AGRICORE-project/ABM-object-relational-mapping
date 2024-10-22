using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing greening farm year subsidies.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class GreeningFarmYearController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<GreeningFarmYearData> _repositoryGreeningFarmYear;
        private readonly IMapper _mapper;
        private readonly ILogger<GreeningFarmYearController> _logger;

        public GreeningFarmYearController(
            IRepository<Population> repositoryPopulation,
            IRepository<Year> repositoryYear,
            IRepository<GreeningFarmYearData> repositoryGreeningFarmYear,
            IMapper mapper,
            ILogger<GreeningFarmYearController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryYear = repositoryYear;
            _repositoryGreeningFarmYear = repositoryGreeningFarmYear;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new greening farm year data entry.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The farm code.</param>
        /// <param name="GreeningFarmYear">The greening farm year data to add.</param>
        /// <returns>Returns the added greening farm year data on success.</returns>

        [HttpPost("/population/{populationId}/farms/{farmCode}/GreeningFarmYear/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GreeningFarmYearData>> AddGreeningFarmYear(long populationId, string farmCode, GreeningFarmYearDataCreateDTO GreeningFarmYear)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid state: "+ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Farms).ThenInclude(f => f.HoldersFarmYearData));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            var existingFarm = existingPopulation.Farms?.Find(f => f.FarmCode == farmCode);//await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farmCode && f.PopulationId == populationId);
            if (existingFarm == null)
            {
                error = "This farm does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            var existingHolder = existingFarm.HoldersFarmYearData?.Find(h => h.YearId == GreeningFarmYear.YearId); //await _repositoryGreeningFarmYear.GetSingleOrDefaultAsync(h => h.FarmId == existingFarm.Id && h.YearId == GreeningFarmYear.YearId) != null;
            if (existingHolder != null)
            {
                error = "This farm already has a holder for this year";
                _logger.LogError(error);
                return StatusCode(400, error);
            }

            GreeningFarmYearData newGreeningFarmYear = _mapper.Map<GreeningFarmYearData>(GreeningFarmYear);
            newGreeningFarmYear.FarmId = existingFarm.Id;

            var(success,message) = await _repositoryGreeningFarmYear.AddAsync(newGreeningFarmYear);
            if (success)
            {
                _logger.LogInformation($"GreeningFarmYear {newGreeningFarmYear.Id} added");
                return CreatedAtAction(nameof(AddGreeningFarmYear), new { id = newGreeningFarmYear.Id }, newGreeningFarmYear);
            }
            else
            {
                _logger.LogError("Error while inserting GreeningFarmYear: " + message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds a range of greening farm year data entries.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The farm code.</param>
        /// <param name="GreeningFarmYears">The list of greening farm year data to add.</param>
        /// <returns>Returns the IDs of the created greening farm year data entries on success.</returns>

        [HttpPost("/population/{populationId}/farms/{farmCode}/GreeningFarmYear/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<GreeningFarmYearData>>> AddGreeningFarmYearRange(long populationId, string farmCode, List<GreeningFarmYearDataCreateDTO> GreeningFarmYears)
        {
            //checks if the model in the request is correct
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            //check if there is any duplicated Year value
            var duplicateYears = GreeningFarmYears
                .GroupBy(GreeningFarmYear => GreeningFarmYear.YearId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();
            string error = string.Empty;
            if (duplicateYears.Any())
            {
                error = $"Duplicate years found: {string.Join(", ", duplicateYears)}";
                _logger.LogError(error);
                return BadRequest(error);
            }

            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId,include: p => p.Include(p => p.Farms));
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            var existingFarm = existingPopulation.Farms?.Find(f => f.FarmCode == farmCode); //await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farmCode && f.PopulationId == populationId);
            if (existingFarm == null)
            {
                error = "This farm does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            List<GreeningFarmYearData> newGreeningFarmYears = GreeningFarmYears.ConvertAll(x => _mapper.Map<GreeningFarmYearData>(x));
            newGreeningFarmYears.ForEach(h => h.FarmId = existingFarm.Id);
            var(success,message) = await _repositoryGreeningFarmYear.AddRangeAsync(newGreeningFarmYears);
            //return array with created entries IDs
            if (success)
            {
                var createdHolderIds = newGreeningFarmYears.Select(h => h.Id).ToList();
                var result = new { ids = createdHolderIds };
                _logger.LogInformation($"GreeningFarmYear {String.Join(",",createdHolderIds)} added");
                return CreatedAtAction(nameof(AddGreeningFarmYearRange), new { }, result);
            }
            else
            {
                _logger.LogError("Error while inserting GreeningFarmYear: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all greening farm year data entries for a specific farm.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The farm code.</param>
        /// <returns>Returns the list of greening farm year data entries for the farm.</returns>

        [HttpGet("/population/{populationId}/farms/{farmCode}/GreeningFarmYear/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<GreeningFarmYearData>>> GetAllGreeningFarmYear(long populationId, string farmCode)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Farms).ThenInclude(f => f.HoldersFarmYearData));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var existingFarm = existingPopulation.Farms?.Find(f => f.FarmCode == farmCode); //await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farmCode && f.PopulationId == populationId);
            if (existingFarm == null)
            {
                error = "This farm does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var result = existingFarm.HoldersFarmYearData ; //await _repositoryGreeningFarmYear.GetAllAsync(h => h.FarmId == existingFarm.Id);
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("No GreeningFarmYear was found");
                return new NoContentResult();
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific greening farm year data entry by year.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The farm code.</param>
        /// <param name="year">The year of the greening farm data.</param>
        /// <returns>Returns the greening farm year data entry for the specified year.</returns>

        [HttpGet("/population/{populationId}/farms/{farmCode}/GreeningFarmYear/year/{year}/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GreeningFarmYearData>> GetGreeningFarmYearByYear(long populationId, string farmCode, long year)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Farms).ThenInclude(f => f.HoldersFarmYearData));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var existingFarm = existingPopulation.Farms?.Find(f => f.FarmCode == farmCode);//await _repositoryFarm.GetSingleOrDefaultAsync(f => f.FarmCode == farmCode && f.PopulationId == populationId);
            if (existingFarm == null)
            {
                error = "This farm does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var result = existingFarm.HoldersFarmYearData?.Find(h => h.YearId == year);//await _repositoryGreeningFarmYear.GetSingleOrDefaultAsync(h => h.FarmId == existingFarm.Id && h.YearId == year);
            if (result == null)
            {
                _logger.LogInformation("No GreeningFarmYear was found");
                return new NoContentResult();
            }
            return Ok(result);
        }


    }
}
