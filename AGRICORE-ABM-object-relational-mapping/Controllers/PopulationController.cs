using AGRICORE_ABM_object_relational_mapping.Helpers;
using AGRICORE_ABM_object_relational_mapping.Services;
using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing populations and related operations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PopulationController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;

        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IRepository<FADNProductRelation> _repositoryFADNProductRelation;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IJsonObjService _jsonObjService;
        private readonly ILogger<PopulationController> _logger;

        private readonly IMapper _mapper;

        public PopulationController (
            IRepository<Population> repositoryPopulation,
            IRepository<FADNProduct> repositoryFADNProduct,
            IRepository<Policy> repositoryPolicy,
            IRepository<ProductGroup> repositoryProductGroup,
            IRepository<FADNProductRelation> repositoryFADNProductRelation,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
            IJsonObjService jsonObjService,
            ILogger<PopulationController> logger,
            IMapper mapper
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryFADNProduct = repositoryFADNProduct;
            _repositoryPolicy = repositoryPolicy;
            _repositoryProductGroup = repositoryProductGroup;
            _repositoryFADNProductRelation = repositoryFADNProductRelation;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _jsonObjService = jsonObjService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new population.
        /// </summary>
        /// <param name="population">Population data to add.</param>
        /// <returns>The added population.</returns>
        [HttpPost("/population/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Population>> AddPopulation(Population population)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");
                return BadRequest(ModelState);
            }


            var(success,message) = await _repositoryPopulation.AddAsync(population);
            if (success)
            {
                _logger.LogInformation($"Population {population.Id} added");
                return CreatedAtAction(nameof(AddPopulation), new { id = population.Id }, population);
            }
            else
            {
                _logger.LogError("Error while inserting population: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds partial data (farms) to an existing population.
        /// </summary>
        /// <param name="populationId">ID of the population to update.</param>
        /// <param name="farms">List of farms to add.</param>
        /// <returns>The updated population with added farms.</returns>

        [HttpPost("/population/{populationId}/addPartialData")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Population>> AddPartialDataToPopulation(long populationId, List<FarmJsonDTO> farms)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");

                return BadRequest(ModelState);
            }

            var population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;
            if (population == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            var (success,message) = await _jsonObjService.ImportPartialFarmsIntoPopulationFromJson(populationId, farms);
            if(!success)
            {
                 error = "Error while updating Population " + populationId + ": " +message;
                _logger.LogError(error);
                return BadRequest(error);
            }
            _logger.LogInformation($"Population {populationId} updated");
            return Ok(population);  
        }

        /// <summary>
        /// Adds a range of populations.
        /// </summary>
        /// <param name="populations">List of populations to add.</param>
        /// <returns>The added populations.</returns>
        [HttpPost("/population/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Population>> AddPopulationRange(List<Population> populations)
        {
            //checks if the model in the request is correct
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");

                return BadRequest(ModelState);
            }


            var(success,message) = await _repositoryPopulation.AddRangeAsync(populations);
            if(success) {
                var createdPopulationsIds = populations.Select(population => population.Id).ToList();

                //return array with created entries IDs
                var result = new { ids = createdPopulationsIds };
                _logger.LogInformation($"Population {String.Join(", ", createdPopulationsIds)} added");

                return CreatedAtAction(nameof(AddPopulationRange), result, _mapper.Map<List<PopulationJsonDTO>>(populations));
            }
            else
            {
                _logger.LogError("Error while inserting Populations: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all populations.
        /// </summary>
        /// <returns>All populations in the database.</returns>
        [HttpGet("/population/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<Population>>> GetAllPopulations()
        {

            var result = await _repositoryPopulation.GetAllAsync();
            string error = string.Empty;
            if (result == null || result.Count == 0)
            {
                error = "There are no populations in the database";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific population by its ID.
        /// </summary>
        /// <param name="populationId">ID of the population to retrieve.</param>
        /// <returns>The population with the specified ID.</returns>
        [HttpGet("/population/{populationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<Population>>> GetPopulation(long populationId)
        {
            var result = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;    
            if (result == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates the description of a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to update.</param>
        /// <param name="description">New description for the population.</param>
        /// <returns>The updated population.</returns>
        [HttpPut("/population/{populationId}/{description}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePopulation(long populationId, string description)
        {
            var result = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;
            if (result == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            result.Description = description;

            var(success,message) = _repositoryPopulation.Update(result);
            if(success) {
                _logger.LogInformation($"Population {populationId} updated");
                return Ok(result);
            }
            else
            {
                _logger.LogError("Error while updating Population: "+message);
                return BadRequest(message);
            }
            
        }
        /// <summary>
        /// Exports all populations to JSON format.
        /// </summary>
        /// <returns>List of JSON representations of all populations.</returns>

        [HttpGet("/population/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IAsyncEnumerable<PopulationJsonDTO>>> ExportPopulations()
        {
            var populations = await _repositoryPopulation.GetAllAsync(include: q => q.Include(q => q.Years));
            if (populations == null || populations.Count == 0)
            {
                _logger.LogInformation("There are no populations in the database");
                return new NoContentResult();
            }

            List<PopulationJsonDTO> result = new List<PopulationJsonDTO>();
            foreach (Population p in populations)
            {
                List<long> yearIds = p.Years.Select(q => q.Id).ToList();
                (PopulationJsonDTO pop, var _) = await _jsonObjService.ExportPopulation(p.Id, yearIds);
                if(pop == null)
                {
                    _logger.LogError($"Error while exporting population {p.Id}");
                    return BadRequest($"Error while exporting population {p.Id}");
                }
                result.Add(pop);
                _logger.LogInformation($"Population {p.Id} exported");
            }
            return Ok(result);
        }

        /// <summary>
        /// Exports a specific population to JSON format by its ID.
        /// </summary>
        /// <param name="populationId">ID of the population to export.</param>
        /// <returns>JSON representation of the specified population.</returns>

        [HttpGet("/population/{populationId}/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<PopulationJsonDTO>> ExportPopulationById(long populationId)
        {
            var population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: q => q.Include(q => q.Years));
            if (population == null)
            {
                _logger.LogInformation("There are no populations in the database");
                return new NoContentResult();
            }

            List<long> yearIds = population.Years.Select(q => q.Id).ToList();

            (PopulationJsonDTO pop, var _) = await _jsonObjService.ExportPopulation(populationId, yearIds);
            if(pop == null)
            {
                _logger.LogError($"Error while exporting population {populationId}");
                return BadRequest($"Error while exporting population {populationId}");
            }
            _logger.LogInformation($"Population {population.Id} exported");

            return Ok(pop);
        }

        /// <summary>
        /// Imports a population from JSON format.
        /// </summary>
        /// <param name="pjson">JSON representation of the population to import.</param>
        /// <returns>The imported population.</returns>
        [HttpPost("/population/import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Population>> ImportPopulation(PopulationJsonDTO pjson)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");

                return BadRequest(ModelState);
            }

            var newPopulationId = await _jsonObjService.ImportPopulationFromJson(pjson);
            string error = string.Empty;
            if (newPopulationId == 0)
            {
                error = "Error while importing population";
                _logger.LogError(error);
                return StatusCode(500, error);
            }
            var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == newPopulationId);
            _logger.LogInformation($"Population {newPopulation.Id} imported");
            return CreatedAtAction(nameof(ImportPopulation), new { id = newPopulation.Id }, newPopulation);
        }

        /// <summary>
        /// Imports multiple populations from JSON format.
        /// </summary>
        /// <param name="pjsonList">List of JSON representations of populations to import.</param>
        /// <returns>List of imported populations.</returns>
        [HttpPost("/population/import/multiple")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<Population>>> ImportPopulations(List<PopulationJsonDTO> pjsonList)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid model state: {ErrorHelper.GetErrorDescription(ModelState)}");

                return BadRequest(ModelState);
            }

            List<Population> createdPList = new List<Population>();
            string error = string.Empty;
            foreach (PopulationJsonDTO pjson in pjsonList)
            {
                var newPopulationId = await _jsonObjService.ImportPopulationFromJson(pjson);

                if(newPopulationId == 0)
                {
                    error = "Error while importing population";
                    _logger.LogError(error);
                    return StatusCode(500, error);
                }
                var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == newPopulationId);
                _logger.LogInformation($"Population {newPopulation.Id} imported");

                createdPList.Add(newPopulation);
            }
            var createdPIds = createdPList.Select(p => p.Id).ToList();
            return CreatedAtAction(nameof(ImportPopulations), createdPIds, createdPList);
        }
    }
}
