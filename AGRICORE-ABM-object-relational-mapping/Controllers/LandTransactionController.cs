using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing land transactions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LandTransactionController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<LandTransaction> _repositoryLandTransaction;
        private readonly IMapper _mapper;
        private readonly ILogger<LandTransactionController> _logger;

        public LandTransactionController(
            IRepository<Population> repositoryPopulation,
            IRepository<LandTransaction> repositoryLandTransaction,
            IMapper mapper,
            ILogger<LandTransactionController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryLandTransaction = repositoryLandTransaction;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports land transactions for a specific population.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="landTransactionDTOs">List of land transaction data to import.</param>
        /// <returns>Created land transaction entries.</returns>

        [HttpPost("/landtransactions/import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LandTransaction>> ImportLandTransactions([FromQuery] long populationId, List<LandTransactionJsonDTO> landTransactionDTOs)
        {
            var population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
            .Include(p => p.Farms).ThenInclude(f => f.AgriculturalProductions).ThenInclude(agp => agp.ProductGroup)
            .Include(p => p.Years)
            , asNoTracking:true);

            string error = string.Empty;
            if(population == null)
            {
                error = "This populations does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            List<LandTransaction> landTransactions = new List<LandTransaction>();
            List<AgriculturalProduction> agriculturalProductions = new List<AgriculturalProduction>();
            population.Farms.ForEach(f => agriculturalProductions.AddRange(f.AgriculturalProductions));

            foreach ( var landTransactionDTO in landTransactionDTOs)
            {
                landTransactions.Add(new LandTransaction
                {
                    DestinationFarmId = population.Farms.FirstOrDefault(f => f.FarmCode == landTransactionDTO.DestinationFarmCode)?.Id ?? 0,
                    Percentage = landTransactionDTO.Percentage,
                    SalePrice = landTransactionDTO.SalePrice,
                    YearId = population.Years.FirstOrDefault(y => y.YearNumber == landTransactionDTO.YearNumber)?.Id ?? 0,
                    ProductionId = agriculturalProductions.FirstOrDefault(agp => agp.ProductGroup.Name == landTransactionDTO.ProductGroupName)?.Id ?? 0
                });
            }
            var(success, message) = await _repositoryLandTransaction.AddRangeAsync(landTransactions);
            if (success)
            {
                var createdIds = landTransactions.Select(x => x.Id).ToList();
                _logger.LogInformation($"LandTransactions {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(ImportLandTransactions), landTransactions);
            }
                
            else
            {
                _logger.LogError("Error while inserting LandtRansactions: "+message);
                return BadRequest(message);
            }
            
        }

        /// <summary>
        /// Exports land transactions for a specific population and year.
        /// </summary>
        /// <param name="populationId">The ID of the population.</param>
        /// <param name="year">The year to export land transactions.</param>
        /// <returns>List of exported land transaction data.</returns>

        [HttpGet("/landtransactions/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<LandTransactionJsonDTO>>> ExportLandTransactions([FromQuery ]long populationId, [FromQuery] long year)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
            .Include(p => p.Farms).ThenInclude(f => f.LandTransactions.Where(l => l.Year.YearNumber == year)).ThenInclude(l => l.Production).ThenInclude(ap => ap.ProductGroup)
            .Include(p => p.Farms).ThenInclude(f => f.LandTransactions.Where(l => l.Year.YearNumber == year)).ThenInclude(l => l.Year)
            .Include(p => p.Farms).ThenInclude(f => f.LandTransactions.Where(l => l.Year.YearNumber == year)).ThenInclude(l => l.Production).ThenInclude(ap => ap.Farm)
            , asNoTracking:true,asSeparateQuery:true);

            string error= string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(409, error);
            }

            List<LandTransaction> landTransactions = new List<LandTransaction>();

            foreach (var farm in existingPopulation.Farms)
            {
                if(farm.LandTransactions != null)
                    landTransactions.AddRange(farm.LandTransactions);
            };

            List<LandTransactionJsonDTO> mappedLandTransactions = _mapper.Map<List<LandTransactionJsonDTO>>(landTransactions);
            return Ok(mappedLandTransactions);
        }
    }
}
