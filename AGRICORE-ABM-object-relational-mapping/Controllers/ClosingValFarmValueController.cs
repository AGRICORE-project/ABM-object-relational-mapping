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
    /// Controller for managing closing value of farm values.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ClosingValFarmValueController : ControllerBase
    {

        private readonly IRepository<ClosingValFarmValue> _repositoryClosingValFarmValue;
        private readonly ILogger<ClosingValFarmValueController> _logger;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IMapper _mapper;


        
        public ClosingValFarmValueController(
            IRepository<ClosingValFarmValue> repositoryClosingValFarmValue,
            IRepository<Farm> repositoryFarm,
            IRepository<Year> repositoryYear,
            IMapper mapper,
            ILogger<ClosingValFarmValueController> logger
        )
        {
            _repositoryClosingValFarmValue = repositoryClosingValFarmValue;
            _repositoryFarm = repositoryFarm;
            _repositoryYear = repositoryYear;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a single closing value of farm value entry to the database.
        /// </summary>
        /// <param name="value">The closing value of farm value object to add.</param>
        /// <returns>Returns the added closing value of farm value on success.</returns>

        [HttpPost("/closingValFarmValues/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClosingValFarmValue>> AddClosingValFarmValue(ClosingValFarmValue value)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");
                return BadRequest(ModelState);
            }
            string error = string.Empty;
            //checks if a farm with that id exists
            var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(farm => farm.Id == value.FarmId);
            if (existingFarm == null)
            {
                error = $"Given Farm wasn't found ({value.FarmId})";
                _logger.LogError(error);
                return StatusCode(404, error);
            }
            //check if a year with that id exists
            var anyYear = await _repositoryYear.GetSingleOrDefaultAsync(year => year.Id == value.YearId && year.PopulationId == existingFarm.PopulationId);
            if (anyYear == null)
            {
                error = $"Given YearId ({value.YearId}) wasn't found, or it doesn't exist for the same population ({existingFarm.PopulationId}) as the Farm";
                _logger.LogError(error);
                return StatusCode(404, error);
            }
            //check if there is already a row with that same year and farm
            var anypreviousdata = await _repositoryClosingValFarmValue.GetSingleOrDefaultAsync(row => row.FarmId == value.FarmId && row.YearId == value.YearId);
            if (anypreviousdata == null)
            {
                var data = _mapper.Map<ClosingValFarmValue>(value);
                await _repositoryClosingValFarmValue.AddAsync(data);
                _logger.LogInformation($"ClosingValFarmValue {data.Id} added");
                return CreatedAtAction(nameof(AddClosingValFarmValue), new { id = data.Id }, data);
            }
            error = "There might be an entry for that input already.";
            _logger.LogError(error);
            return BadRequest(error);

        }

        /// <summary>
        /// Adds a range of closing values of farm value entries to the database.
        /// </summary>
        /// <param name="values">List of closing value of farm value objects to add.</param>
        /// <returns>Returns the added closing values of farm value on success.</returns>

        [HttpPost("/closingValFarmValues/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClosingValFarmValue>> AddClosingValFarmValueRange(List<ClosingValFarmValue> values)
        {
            foreach (var value in values)
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Invalid model state: { ErrorHelper.GetErrorDescription(ModelState)}");
                    return BadRequest(ModelState);
                }
                string error = string.Empty;
                //checks if a farm with that id exists
                var anyFarm = await _repositoryFarm.GetSingleOrDefaultAsync(farm => farm.Id == value.FarmId);
                //check if a year with that id exists
                var anyYear = anyFarm != null ? await _repositoryYear.GetSingleOrDefaultAsync(year => year.Id == value.YearId && year.PopulationId == anyFarm!.PopulationId) : null;
                //check if there is already a row with that same year and farm
                var anypreviousdata = await _repositoryClosingValFarmValue.GetAllAsync(row => row.FarmId == value.FarmId && row.YearId == value.YearId);
                if (anyFarm == null || anyYear == null || anypreviousdata != null)
                {
                    error = "Either YearId or FarmId does not exist, or there might be an entry for that same input already.";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
            }
            var data = _mapper.Map<List<ClosingValFarmValue>>(values);
            var(success,message) = await _repositoryClosingValFarmValue.AddRangeAsync(data);
            if (success)
            {
                var createdIds = values.Select(x => x.Id).ToList();
                _logger.LogInformation($"ClosingValFarmValues {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddClosingValFarmValueRange), new { }, data);
            }
            else
            {
                _logger.LogError("Error while inserting ClosingValFarmValues: " + message);
                return BadRequest(message);
            }
            
        }

        /// <summary>
        /// Retrieves all closing values of farm values from the database.
        /// </summary>
        /// <returns>Returns a list of closing values of farm values on success.</returns>

        [HttpGet("/closingValFarmValues/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<ClosingValFarmValue>>> GetAllClosingValFarmValueRange()
        {
            List<ClosingValFarmValue> farmsData = await _repositoryClosingValFarmValue.GetAllAsync(include: ob => ob.Include(ob => ob.Farm).Include(ob => ob.Year), asNoTracking: true, asSeparateQuery: true);

            if (farmsData == null || farmsData.Count == 0)
            {
                _logger.LogInformation("There are no ClosingValFarmValue in the database");
                return new NoContentResult();
            }
            return Ok(farmsData);
        }

        /// <summary>
        /// Retrieves closing values of farm values from the database for a specific year.
        /// </summary>
        /// <param name="year">The year for which closing values of farm values are to be retrieved.</param>
        /// <returns>Returns a list of closing values of farm values for the specified year on success.</returns>

        [HttpGet("/closingValFarmValuesByYear/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<ClosingValFarmValue>>> GetAllClosingValFarmValueRange(int year)
        {
            List<ClosingValFarmValue> farmsData = await _repositoryClosingValFarmValue.GetAllAsync(include: ob => ob.Include(ob => ob.Farm).Include(ob => ob.Year), asNoTracking: true, asSeparateQuery: true);

            // Filter the farmsData list to include only the data with the specified year

            var existingYear = await _repositoryYear.GetAllAsync(y => y.YearNumber == year);
            string error = string.Empty;

            if (existingYear == null || existingYear.Count == 0)
            {
                error = "This year doesn't exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            farmsData = farmsData.Where(data => existingYear.Any(y => y.Id == data.YearId)).ToList();

            if (farmsData.Count == 0)
            {
                error = "No ClosingValFarmValue found for the specified year";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            return Ok(farmsData);
        }

    }
}
