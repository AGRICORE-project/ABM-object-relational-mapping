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
    /// Controller for managing holder farm year data.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HolderFarmYearDataController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<HolderFarmYearData> _repositoryHolderFarmYearData;
        private readonly IMapper _mapper;
        private readonly ILogger<HolderFarmYearDataController> _logger;

        public HolderFarmYearDataController(
            IRepository<Population> repositoryPopulation,
            IRepository<Year> repositoryYear,
            IRepository<HolderFarmYearData> repositoryHolderFarmYearData,
            IMapper mapper,
            ILogger<HolderFarmYearDataController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryYear = repositoryYear;
            _repositoryHolderFarmYearData = repositoryHolderFarmYearData;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new holder farm year data entry for a specific population and farm.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The code of the farm.</param>
        /// <param name="holderFarmYearData">Data to create a new holder farm year entry.</param>
        /// <returns>Created holder farm year data entry.</returns>

        [HttpPost("/population/{populationId}/farms/{farmCode}/HolderData/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HolderFarmYearData>> AddHolderFarmYearData(long populationId, string farmCode, HolderFarmYearDataCreateDTO holderFarmYearData)
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

            var existingHolder = existingFarm.HoldersFarmYearData?.Find(h => h.YearId == holderFarmYearData.YearId); //await _repositoryHolderFarmYearData.GetSingleOrDefaultAsync(h => h.FarmId == existingFarm.Id && h.YearId == holderFarmYearData.YearId) != null;
            if (existingHolder != null)
            {
                error = "This farm already has a holder for this year";
                _logger.LogError(error);
                return StatusCode(400, error);
            }

            HolderFarmYearData newHolderFarmYearData = _mapper.Map<HolderFarmYearData>(holderFarmYearData);
            newHolderFarmYearData.FarmId = existingFarm.Id;

            var(success,message) = await _repositoryHolderFarmYearData.AddAsync(newHolderFarmYearData);
            if (success)
            {
                _logger.LogInformation($"HolderFarmYearData {newHolderFarmYearData.Id} added");
                return CreatedAtAction(nameof(AddHolderFarmYearData), new { id = newHolderFarmYearData.Id }, newHolderFarmYearData);
            }
            else
            {
                _logger.LogError("Error while inserting HolderFarmYearData: " + message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds a range of holder farm year data entries for a specific population and farm.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The code of the farm.</param>
        /// <param name="holderFarmYearDatas">List of data to create multiple holder farm year entries.</param>
        /// <returns>Array with IDs of created holder farm year data entries.</returns>

        [HttpPost("/population/{populationId}/farms/{farmCode}/HolderData/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<HolderFarmYearData>>> AddHolderFarmYearDataRange(long populationId, string farmCode, List<HolderFarmYearDataCreateDTO> holderFarmYearDatas)
        {
            //checks if the model in the request is correct
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            //check if there is any duplicated Year value
            var duplicateYears = holderFarmYearDatas
                .GroupBy(holderFarmYearData => holderFarmYearData.YearId)
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

            List<HolderFarmYearData> newHolderFarmYearDatas = holderFarmYearDatas.ConvertAll(x => _mapper.Map<HolderFarmYearData>(x));
            newHolderFarmYearDatas.ForEach(h => h.FarmId = existingFarm.Id);
            var(success,message) = await _repositoryHolderFarmYearData.AddRangeAsync(newHolderFarmYearDatas);
            //return array with created entries IDs
            if (success)
            {
                var createdHolderIds = newHolderFarmYearDatas.Select(h => h.Id).ToList();
                var result = new { ids = createdHolderIds };
                _logger.LogInformation($"HolderFarmYearData {String.Join(",",createdHolderIds)} added");
                return CreatedAtAction(nameof(AddHolderFarmYearDataRange), new { }, result);
            }
            else
            {
                _logger.LogError("Error while inserting HolderFarmYearData: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all holder farm year data entries for a specific population and farm.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The code of the farm.</param>
        /// <returns>List of holder farm year data entries.</returns>

        [HttpGet("/population/{populationId}/farms/{farmCode}/HolderData/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<HolderFarmYearData>>> GetAllHolderFarmYearData(long populationId, string farmCode)
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

            var result = existingFarm.HoldersFarmYearData ; //await _repositoryHolderFarmYearData.GetAllAsync(h => h.FarmId == existingFarm.Id);
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("No HolderFarmYearData was found");
                return new NoContentResult();
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific holder farm year data entry by year for a given population and farm.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="farmCode">The code of the farm.</param>
        /// <param name="year">The year to filter holder farm year data.</param>
        /// <returns>Holder farm year data entry for the specified year.</returns>

        [HttpGet("/population/{populationId}/farms/{farmCode}/HolderData/year/{year}/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HolderFarmYearData>> GetHolderFarmYearDataByYear(long populationId, string farmCode, long year)
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

            var result = existingFarm.HoldersFarmYearData?.Find(h => h.YearId == year);//await _repositoryHolderFarmYearData.GetSingleOrDefaultAsync(h => h.FarmId == existingFarm.Id && h.YearId == year);
            if (result == null)
            {
                _logger.LogInformation("No HolderFarmYearData was found");
                return new NoContentResult();
            }
            return Ok(result);
        }


    }
}
