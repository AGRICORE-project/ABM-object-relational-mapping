using AGRICORE_ABM_object_relational_mapping.Services;
using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing synthetic populations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SyntheticPopulationController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<SyntheticPopulation> _repositorySyntheticPopulation;
        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IRepository<FADNProductRelation> _repositoryFADNProductRelation;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IJsonObjService _jsonObjService;
        private readonly IPopulationDuplicationService _PopulationDuplicationService;
        private readonly IMapper _mapper;
        private readonly ILogger<SyntheticPopulationController> _logger;

        public SyntheticPopulationController(
            IRepository<Population> repositoryPopulation,
            IRepository<Year> repositoryYear,
            IRepository<SyntheticPopulation> repositorySyntheticPopulation,
            IRepository<FADNProduct> repositoryFADNProduct,
            IRepository<Policy> repositoryPolicy,
            IRepository<ProductGroup> repositoryProductGroup,
            IRepository<FADNProductRelation> repositoryFADNProductRelation,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
            IJsonObjService jsonObjService,
            IPopulationDuplicationService populationDuplicationService,
            IMapper mapper,
            ILogger<SyntheticPopulationController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryYear = repositoryYear;
            _repositorySyntheticPopulation = repositorySyntheticPopulation;
            _repositoryFADNProduct = repositoryFADNProduct;
            _repositoryPolicy = repositoryPolicy;
            _repositoryProductGroup = repositoryProductGroup;
            _repositoryFADNProductRelation = repositoryFADNProductRelation;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _jsonObjService = jsonObjService;
            _PopulationDuplicationService = populationDuplicationService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new synthetic population.
        /// </summary>
        /// <param name="syntheticPopulation">Synthetic population data to add.</param>
        /// <returns>The added synthetic population.</returns>
        [HttpPost("/synthetic/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SyntheticPopulation>> AddSyntheticPopulation(SyntheticPopulation syntheticPopulation)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: "+ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            string error = string.Empty;
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == syntheticPopulation.PopulationId, include: p => p.Include(p => p.Years));

            if (existingPopulation == null)
            {
                error = "This population doesn't exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            if (existingPopulation?.Years == null || !existingPopulation.Years.Any(y => y.PopulationId == syntheticPopulation.PopulationId))
            {
                error = "The related year doesn't exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            var existingSynthetic = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(sp => sp.PopulationId == syntheticPopulation.PopulationId && sp.YearId == syntheticPopulation.YearId);

            if (existingSynthetic != null)
            {
                error = "This synthetic population already exist";
                _logger.LogError(error);
                return BadRequest(error);
            }

            syntheticPopulation.PopulationId = syntheticPopulation.PopulationId;
            syntheticPopulation.YearId = syntheticPopulation.YearId;

            var(success,message) = await _repositorySyntheticPopulation.AddAsync(syntheticPopulation);
            if (!success)
            {
                _logger.LogError("Error while inserting SyntheticPopulation: "+message);
            }
            _logger.LogInformation($"SyntheticPopulation {syntheticPopulation.Id} added");
            return CreatedAtAction(nameof(AddSyntheticPopulation), new { id = syntheticPopulation.Id }, syntheticPopulation);
        }

        /// <summary>
        /// Retrieves all synthetic populations.
        /// </summary>
        /// <returns>List of synthetic populations.</returns>
        [HttpGet("/synthetic/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SyntheticPopulation>>> GetAllSyntheticPopulations()
        {

            var result = await _repositorySyntheticPopulation.GetAllAsync();
            string error = string.Empty;
            if (result == null || result.Count == 0)
            {
                error = "No entities found.";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific synthetic population by its ID.
        /// </summary>
        /// <param name="syntheticId">ID of the synthetic population to retrieve.</param>
        /// <returns>The synthetic population.</returns>
        [HttpGet("/synthetic/{syntheticId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SyntheticPopulation>>> GetSyntheticPopulation(long syntheticId)
        {
            var result = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(sp => sp.Id == syntheticId);
            string error = string.Empty;
            if (result == null)
            {
                error = $"Synthetic population {syntheticId} does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates a specific synthetic population by its ID.
        /// </summary>
        /// <param name="syntheticId">ID of the synthetic population to update.</param>
        /// <param name="description">New description of the synthetic population.</param>
        /// <param name="name">New name of the synthetic population.</param>
        /// <returns>The updated synthetic population.</returns>
        [HttpPut("/synthetic/{syntheticId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateSyntheticPopulation(long syntheticId, [FromQuery]string description, [FromQuery]string name)
        {
            var result = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(sp => sp.Id == syntheticId);
            string error = string.Empty;
            if (result == null)
            {
                error = $"Synthetic population {syntheticId} does not exist";
                _logger.LogError(error);
                return StatusCode(404, "No entities found.");
            }

            result.Description = description;
            result.Name = name;

            var(success,message) = _repositorySyntheticPopulation.Update(result);
            if (!success)
            {
                error = $"Error updating Synthetic population {syntheticId}";
                _logger.LogError(error);
                return BadRequest(error);
            }
            _logger.LogInformation($"Synthetic population {syntheticId} updated");
            return Ok(result);
        }

        /// <summary>
        /// Duplicates a specific synthetic population by its ID.
        /// </summary>
        /// <param name="syntheticId">ID of the synthetic population to duplicate.</param>
        /// <returns>The duplicated synthetic population.</returns>
        [HttpPost("/synthetic/{syntheticId}/duplicate")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SyntheticPopulation>> DuplicateSyntheticPopulation(long syntheticId)
        {
            var result = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(sp => sp.Id == syntheticId);
            string error = string.Empty;
            if (result == null)
            {
                error = $"Synthetic population {syntheticId} does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            long newPopulationId = await _PopulationDuplicationService.CreatePopulationForSimulation(syntheticId);
            var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == newPopulationId);
            if (newPopulation == null)
            {
                error = $"Error while duplicating population";
                _logger.LogError(error);
                return StatusCode(400, error);
            }
            newPopulation.Description = newPopulation.Description.Replace("Replicated", "Duplicated");
            var updateResult =  _repositoryPopulation.Update(newPopulation);
            if (!updateResult.Item1)
            {
                _logger.LogError(updateResult.Item2);
                return StatusCode(400, updateResult.Item2);
            }
            SyntheticPopulation newSyntheticPopulation = new SyntheticPopulation
            {
                Description = result.Description,
                Name = "Duplicated from SP: " + result.Name,
                PopulationId = newPopulationId,
                YearId = result.YearId,
            };
            var (success, message) = await _repositorySyntheticPopulation.AddAsync(newSyntheticPopulation);
            if (!success)
            {
                _logger.LogError("Error while inserting SyntheticPopulation: " + message);
            }
            newSyntheticPopulation.Name = newSyntheticPopulation.Name + " - " + newSyntheticPopulation.Id;
            _repositorySyntheticPopulation.Update(newSyntheticPopulation);
            _logger.LogInformation($"SyntheticPopulation {newSyntheticPopulation.Id} added");
            return CreatedAtAction(nameof(AddSyntheticPopulation), new { id = newSyntheticPopulation.Id }, newSyntheticPopulation);
        }

        /// <summary>
        /// Exports all synthetic populations.
        /// </summary>
        /// <returns>List of synthetic population DTOs.</returns>

        [HttpGet("/synthetic/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<SyntheticPopulationJsonDTO>>> ExportSyntheticPopulations()
        {
            var syntheticPopulations = await _repositorySyntheticPopulation.GetAllAsync();
            string error = string.Empty;
            if (syntheticPopulations == null || syntheticPopulations.Count == 0)
            {
                error = $"There are no synthetic populations in the database";
                _logger.LogInformation(error);
                return new NoContentResult();
            }

            List<SyntheticPopulationJsonDTO> result = new List<SyntheticPopulationJsonDTO>();
            foreach (SyntheticPopulation sp in syntheticPopulations)
            {
                (SyntheticPopulationJsonDTO synthPop, var _) = await _jsonObjService.ExportSyntheticPopulation(sp.Id);
                if(synthPop == null)
                {
                    error = $"Error while exporting synthetic population {sp.Id}";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
                result.Add(synthPop);
            }
            var exportedNames = result.Select(x => x.Name).ToList();
            _logger.LogInformation($"Synthetic populations {System.String.Join(",",exportedNames)} exported");
            return Ok(result);
        }

        /// <summary>
        /// Exports all synthetic populations for a specific population ID.
        /// </summary>
        /// <param name="populationId">ID of the population.</param>
        /// <returns>List of synthetic population DTOs.</returns>
        [HttpGet("/synthetic/export/population/{populationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<SyntheticPopulationJsonDTO>>> ExportSyntheticPopulations(long populationId)
        {
            var syntheticPopulations = await _repositorySyntheticPopulation.GetAllAsync(sp => sp.PopulationId == populationId);
            string error = string.Empty;
            if (syntheticPopulations == null || syntheticPopulations.Count == 0)
            {
                error = $"There are no synthetic populations in the database for the population {populationId}";
                _logger.LogInformation(error);
                return new NoContentResult();
            }
            List<SyntheticPopulationJsonDTO> result = new List<SyntheticPopulationJsonDTO>();
            foreach (SyntheticPopulation sp in syntheticPopulations)
            {
                (SyntheticPopulationJsonDTO synthPop, var _) = await _jsonObjService.ExportSyntheticPopulation(sp.Id);
                if (synthPop == null)
                {
                    error = $"Error while exporting synthetic population {sp.Id}";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
                result.Add(synthPop);
            }
            var createdNames = result.Select(x => x.Name).ToList();
            _logger.LogInformation($"Synthetic populations {System.String.Join(",",createdNames)} exported");
            return Ok(result);
        }

        /// <summary>
        /// Exports a specific synthetic population by its ID.
        /// </summary>
        /// <param name="syntheticId">ID of the synthetic population to export.</param>
        /// <returns>The synthetic population DTO.</returns>
        [HttpGet("/synthetic/export/{syntheticId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SyntheticPopulationJsonDTO>> ExportSyntheticPopulationById(long syntheticId)
        {
            var syntheticPopulation = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(sp => sp.Id == syntheticId);
            if (syntheticPopulation == null)
            {
                _logger.LogInformation($"Synthetic population {syntheticId} does not exist");
                return new NoContentResult();
            }

            (SyntheticPopulationJsonDTO result, var _) = await _jsonObjService.ExportSyntheticPopulation(syntheticPopulation.Id);
            _logger.LogInformation($"Synthetic populations {result.Name} exported");
            return Ok(result);
        }

        /// <summary>
        /// Imports a synthetic population from a DTO.
        /// </summary>
        /// <param name="spjson">DTO of the synthetic population to import.</param>
        /// <returns>The imported synthetic population.</returns>
        [HttpPost("/synthetic/import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SyntheticPopulation>> ImportSyntheticPopulation(SyntheticPopulationJsonDTO spjson)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            string error = string.Empty;
            long newPopulationId = await _jsonObjService.ImportPopulationFromSPJson(spjson);

            if (newPopulationId == 0)
            {
                error = $"Error while importing population from synthetic population {spjson.Name}";
                _logger.LogError(error);
                return StatusCode(500, error);
            }

            var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == newPopulationId, include: p => p.Include(p => p.SyntheticPopulations).Include(p => p.Years), asSeparateQuery: true);
            SyntheticPopulation synthetic = new SyntheticPopulation
            {
                Description = spjson.Description,
                Name = spjson.Name,
                PopulationId = newPopulationId,
                YearId = newPopulation.Years[0].Id
            };
            var result = await _repositorySyntheticPopulation.AddAsync(synthetic);
            if (result.Item1)
            {
                _logger.LogInformation($"Synthetic population {synthetic.Id} added");
                return CreatedAtAction(nameof(ImportSyntheticPopulation), new { id = synthetic.Id, populationId = newPopulationId }, synthetic);
            }
            else {
                error = $"Error while importing Synthetic population {spjson.Name}";
                _logger.LogError(error);
                return StatusCode(500, error);
            }
        }

        /// <summary>
        /// Imports multiple synthetic populations from a list of DTOs.
        /// </summary>
        /// <param name="spjsonList">List of DTOs of the synthetic populations to import.</param>
        /// <returns>List of imported synthetic populations.</returns>
        [HttpPost("/synthetic/import/multiple")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<SyntheticPopulation>>> ImportSyntheticPopulations(List<SyntheticPopulationJsonDTO> spjsonList)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }

            List<SyntheticPopulation> createdSpList = new List<SyntheticPopulation>();
            List<long> createdSpIds = new List<long>();
            string error = string.Empty;
            foreach (SyntheticPopulationJsonDTO spjson in spjsonList)
            {
                var id = await _jsonObjService.ImportPopulationFromSPJson(spjson);
                if (id == 0)
                {
                    error = $"Error while importing population from synthetic population {spjson.Name}";
                    _logger.LogError(error);
                    return StatusCode(500, error);
                }
                var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == id, include: p => p.Include(p => p.SyntheticPopulations).Include(p => p.Years), asSeparateQuery: true);
                SyntheticPopulation synthetic = new SyntheticPopulation
                {
                    Description = spjson.Description,
                    Name = spjson.Name,
                    PopulationId = id,
                    YearId = newPopulation.Years[0].Id
                };
                var result = await _repositorySyntheticPopulation.AddAsync(synthetic);
                if (result.Item1)
                {
                    _logger.LogInformation($"Synthetic population {synthetic.Id} added");
                    createdSpIds.Add(synthetic.Id);
                    createdSpList.Add(synthetic);
                }
                else
                {
                    error = $"Error while importing Synthetic population {synthetic.Id}";
                    _logger.LogError(error);
                    return StatusCode(500, error);
                }
            }
            return CreatedAtAction(nameof(ImportSyntheticPopulations), createdSpIds, createdSpList);
        }
    }
}
