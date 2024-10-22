using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing land rent data.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LandRentController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<LandRent> _repositoryLandRent;
        private readonly IMapper _mapper;
        private readonly ILogger<LandRentController> _logger;

        public LandRentController(
            IRepository<Population> repositoryPopulation,
            IRepository<LandRent> repositoryLandRent,
            IMapper mapper,
            ILogger<LandRentController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryLandRent = repositoryLandRent;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports land rent data for a specific population.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="LandRentDTOs">List of land rent data to import.</param>
        /// <returns>Created land rent data entries.</returns>

        [HttpPost("/LandRents/import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LandRent>> ImportLandRents([FromQuery] long populationId, List<LandRentJsonDTO> LandRentDTOs)
        {
            var population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
            .Include(p => p.Farms).Include(p => p.Years)
            , asNoTracking:true);

            string error = string.Empty;
            if(population == null)
            {
                error = "This populations does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            List<LandRent> LandRents = new List<LandRent>();

            foreach ( var LandRentDTO in LandRentDTOs)
            {
                LandRents.Add(new LandRent
                {
                    DestinationFarmId = population.Farms.FirstOrDefault(f => f.FarmCode == LandRentDTO.DestinationFarmCode)?.Id ?? 0,
                    OriginFarmId = population.Farms.FirstOrDefault(f => f.FarmCode == LandRentDTO.OriginFarmCode)?.Id ?? 0,
                    YearId = population.Years.FirstOrDefault(y => y.YearNumber == LandRentDTO.YearNumber)?.Id ?? 0,
                    RentArea = LandRentDTO.RentArea,
                    RentValue = LandRentDTO.RentValue
                });
            }
            var(success, message) = await _repositoryLandRent.AddRangeAsync(LandRents);
            if (success)
            {
                var createdIds = LandRents.Select(x => x.Id).ToList();
                _logger.LogInformation($"LandRents {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(ImportLandRents), LandRents);
            }
                
            else
            {
                _logger.LogError("Error while inserting LandRents: "+message);
                return BadRequest(message);
            }
            
        }

        /// <summary>
        /// Exports land rent data for a specific population and year.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="year">The year to export land rent data.</param>
        /// <returns>List of exported land rent data.</returns>

        [HttpGet("/LandRents/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<LandRentJsonDTO>>> ExportLandRents([FromQuery ]long populationId, [FromQuery] long year)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);

            string error= string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(409, error);
            }

            var landRents = await _repositoryLandRent.GetAllAsync(
                l => l.Year.YearNumber == year && (l.OriginFarm.PopulationId == populationId || l.DestinationFarm.PopulationId == populationId),
                include: l => l.Include(l => l.Year).Include(l => l.OriginFarm).Include( l => l.OriginFarm));
            
            List<LandRentJsonDTO> mappedLandRents = _mapper.Map<List<LandRentJsonDTO>>(landRents);
            return Ok(mappedLandRents);
        }
    }
}
