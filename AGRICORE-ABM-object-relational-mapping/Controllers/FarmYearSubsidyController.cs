using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing farm year subsidies.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FarmYearSubsidyController : ControllerBase
    {
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<FarmYearSubsidy> _repositoryFarmYearSubsidy;
        private readonly ILogger<FarmYearSubsidyController> _logger;

        public FarmYearSubsidyController(
            IRepository<Farm> repositoryFarm,
            IRepository<Year> repositoryYear,
            IRepository<FarmYearSubsidy> repositoryFarmYearSubsidy,
            ILogger<FarmYearSubsidyController>  logger
        )
        {
            _repositoryFarm = repositoryFarm;
            _repositoryYear = repositoryYear;
            _repositoryFarmYearSubsidy = repositoryFarmYearSubsidy;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new farm year subsidy entry.
        /// </summary>
        /// <param name="data">The farm year subsidy data to add.</param>
        /// <returns>Returns the added farm year subsidy on success.</returns>

        [HttpPost("/farmYearSubsidy/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FarmYearSubsidy>> AddFarmYearSubsidy(FarmYearSubsidy data)
        {

            var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.Id == data.FarmId);

            var existingYear = existingFarm != null ? await _repositoryYear.GetSingleOrDefaultAsync(y => y.Id == data.YearId && existingFarm.PopulationId == y.PopulationId) : null;
            string error = string.Empty;
            if (existingYear == null || existingFarm == null)
            {
                error = "This year, farm or population containing both does not exist";
                _logger.LogError(error);
                return BadRequest(error);
            }

            var(success,message) = await _repositoryFarmYearSubsidy.AddAsync(data);
            if (success)
            {
                _logger.LogInformation($"FarmYearSubsidy {data.Id} added");
                return CreatedAtAction(nameof(AddFarmYearSubsidy), new { id = data.Id }, data);
            }
                
            else
            {
                _logger.LogError("Error while inserting FarmYearSubsidy: " + message);
                return BadRequest(error);
            }
        }


        /// <summary>
        /// Adds a range of farm year subsidy entries.
        /// </summary>
        /// <param name="data">The list of farm year subsidies to add.</param>
        /// <returns>Returns the IDs of the created farm year subsidies on success.</returns>

        [HttpPost("/farmYearDecoupledSubsidy/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FarmYearSubsidy>> AddFarmYearSubsidyRange(List<FarmYearSubsidy> data)
        {
            string error = string.Empty;    
            foreach (var item in data)
            {
                var existingFarm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.Id == item.FarmId);

                var existingYear = existingFarm != null ? await _repositoryYear.GetSingleOrDefaultAsync(y => y.Id == item.YearId && existingFarm.PopulationId == y.PopulationId) : null;

                if (existingYear == null || existingFarm == null)
                {
                    error = "One or more years,farms or populations containing both do not exist";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
            }

            var(success,message) = await _repositoryFarmYearSubsidy.AddRangeAsync(data);
            if (success)
            {
                var createdIds = data.Select(x => x.Id).ToList();
                _logger.LogInformation($"FarmYearSubsidies {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddFarmYearSubsidyRange), new { }, data);
            }
                
            else
            {
                _logger.LogError("Error while inserting FarmYearSubsidies: " + message);
                return BadRequest(message);
            }
        }



    }
}
