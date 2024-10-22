using AGRICORE_ABM_object_relational_mapping.Services;
using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging.Abstractions;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing simulation scenarios.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SimulationScenarioController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<SimulationScenario> _repositorySimulationScenario;
        private readonly IRepository<SyntheticPopulation> _repositorySyntheticPopulation;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly ISimulationTasksService _simulationTasksService;
        private readonly IPopulationDuplicationService _populationDuplicationService;
        private readonly IJsonObjService _jsonObjService;
        private readonly IMapper _mapper;
        private ILogger<SimulationScenarioController> _logger;


        public SimulationScenarioController(
            IRepository<Population> repositoryPopulation,
            IRepository<SimulationScenario> repositorySimulationScenario,
            IRepository<SyntheticPopulation> repositorySyntheticPopulation,
            IRepository<Policy> repositoryPolicy,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
            IRepository<ProductGroup> repositoryProductGroup,
            ISimulationTasksService simulationTasksService,
            IPopulationDuplicationService populationDuplicationService,
            IJsonObjService jsonObjService,
            IMapper mapper,
            ILogger<SimulationScenarioController> logger

        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositorySimulationScenario = repositorySimulationScenario;
            _repositorySyntheticPopulation = repositorySyntheticPopulation;
            _simulationTasksService = simulationTasksService;
            _populationDuplicationService = populationDuplicationService;
            _repositoryPolicy = repositoryPolicy;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _repositoryProductGroup = repositoryProductGroup;
            _jsonObjService = jsonObjService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new simulation scenario.
        /// </summary>
        /// <param name="simulationScenario">Simulation scenario data to add.</param>
        /// <returns>The added simulation scenario.</returns>

        [HttpPost("/simulationScenario/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SimulationScenario>> AddSimulationScenario(SimulationScenarioAddDTO simulationScenario)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: "+ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }

            var existingSyntheticPopulation = await _repositorySyntheticPopulation.GetSingleOrDefaultAsync(p => p.Id == simulationScenario.SyntheticPopulationId, include: q => q.Include(q => q.Year));
            string error = string.Empty;
            if (existingSyntheticPopulation == null)
            {
                error = "There is no synthetic population with this id: " + simulationScenario.SyntheticPopulationId;
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            long newPopulationId = await _populationDuplicationService.CreatePopulationForSimulation(simulationScenario.SyntheticPopulationId);

            if (newPopulationId == 0)
            {
                error = "There was an error creating the new population for the SimulationScenario";
                _logger.LogError(error);
                return StatusCode(500, error);
            }

            var newPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == newPopulationId, include: q => q.Include(q => q.Years));

            long yearId = newPopulation.Years.FirstOrDefault(q => q.YearNumber == existingSyntheticPopulation.Year.YearNumber)?.Id ?? 0;

            if (yearId == 0)
            {
                _repositoryPopulation.Remove(newPopulation);
                error = "There was an error creating the new population for the SimulationScenario. The duplicated population do not have the same year";
                _logger.LogError(error);
                return StatusCode(500, error);
            }

            // Need to add here the new Policies and PolicyGroupRelations
            if (simulationScenario.AdditionalPolicies != null && simulationScenario.AdditionalPolicies.Count > 0)
            {
                var productGroups = await _repositoryProductGroup.GetAllAsync(q => q.PopulationId == newPopulationId);

                foreach (var policy in simulationScenario.AdditionalPolicies)
                {
                    var newPolicy = new Policy
                    {
                        IsCoupled = policy.IsCoupled,
                        PolicyIdentifier = policy.PolicyIdentifier,
                        StartYearNumber = policy.StartYearNumber,
                        EndYearNumber = policy.EndYearNumber,
                        PopulationId = newPopulationId,
                        PolicyGroupRelations = new List<PolicyGroupRelation>(),
                        EconomicCompensation = policy.EconomicCompensation,
                        ModelLabel = policy.ModelLabel,
                        PolicyDescription = policy.PolicyDescription,
                    };
                    var (policyAddResult, policyAddMessage) = await _repositoryPolicy.AddAsync(newPolicy);
                    if (!policyAddResult)
                    {
                        _repositoryPopulation.Remove(newPopulation);
                        error = "Failed to add one of the extra policies, undoing changes in populations: " + policyAddMessage;
                        return StatusCode(500, error);
                    }
                    if (newPolicy.IsCoupled && policy.CoupledCompensations != null && policy.CoupledCompensations.Count() > 0)
                    {
                        var newPolicyCreated = await _repositoryPolicy.GetSingleOrDefaultAsync(q => q.PolicyIdentifier == policy.PolicyIdentifier && q.PopulationId == newPopulationId);
                        foreach (var relation in policy.CoupledCompensations)
                        {
                            var productGroup = productGroups.FirstOrDefault(q => q.Name == relation.ProductGroup);
                            if (productGroup == null)
                            {
                                _repositoryPopulation.Remove(newPopulation);
                                error = "Failed to find the product group for the coupled compensation, undoing changes in populations";
                                return StatusCode(500, error);
                            }
                            var newRelation = new PolicyGroupRelation
                            {
                                PolicyId = newPolicyCreated.Id,
                                ProductGroupId = productGroup.Id,
                                EconomicCompensation = relation.EconomicCompensation,
                                PopulationId = newPopulationId
                            };
                            var (relationAddResult, relationAddMessage) = await _repositoryPolicyGroupRelation.AddAsync(newRelation);
                            if (!relationAddResult)
                            {
                                _repositoryPopulation.Remove(newPopulation);
                                error = "Failed to add the coupled compensation, undoing changes in populations: " + relationAddMessage;
                                return StatusCode(500, error);
                            }
                        }
                    }
                }
            }

            var simScenario = _mapper.Map<SimulationScenario>(simulationScenario);
            simScenario.PopulationId = newPopulationId;
            simScenario.YearId = yearId;

            var (result, message) = await _repositorySimulationScenario.AddAsync(simScenario);
            if (!result)
            {
                _repositoryPopulation.Remove(newPopulation);
                error = "Failed to add the simulation Scenario, undoing changes in populations: " + message;
                return StatusCode(500, error);
            }

            _logger.LogInformation($"SimulationScenario {simScenario.Id} added");
            await _simulationTasksService.RunSimulationScenario(simScenario, simulationScenario.QueueSuffix);

            return CreatedAtAction(nameof(AddSimulationScenario), new { id = simScenario.Id }, simScenario);
        }

        /// <summary>
        /// Retrieves all simulation scenarios for a specific population and year.
        /// </summary>
        /// <param name="populationId">ID of the population.</param>
        /// <param name="yearId">ID of the year.</param>
        /// <returns>List of simulation scenarios.</returns>

        [HttpGet("/population/{populationId}/year/{yearId}/simulationScenario/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenario>>> GetAllSimulationScenariosForPopulationAndYear(long populationId, long yearId)
        {

            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p.Include(p => p.Years).Include(p => p.SimulationScenarios), asNoTracking: true);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var existingYear = existingPopulation!.Years?.Find(y => y.Id == yearId);

            if (existingYear == null)
            {
                error = $@"There is no year with this id {yearId} in this population {populationId}";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            var result = existingPopulation!.SimulationScenarios?.FindAll(sc => sc.PopulationId == populationId && sc.YearId == yearId);

            if (result == null || result.Count == 0)
            {
                error = "There are no simulation scenarios in this population for this year";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all simulation scenarios with associated simulation runs.
        /// </summary>
        /// <returns>List of simulation scenarios with associated simulation runs.</returns>

        [HttpGet("/simulationScenario/getWithSimulationRuns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenario>>> GetAllWithSimulationRuns()
        {
            var ss = await _repositorySimulationScenario.GetAllAsync(include: q => q.Include(q => q.SimulationRun), asNoTracking: true);

            if (ss == null || ss.Count == 0)
            {
                string error = "There are no simulation scenarios";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            var result = _mapper.Map<List<SimulationScenarioWithScenarioRunDTO>>(ss);
            foreach (var item in result)
            {
                item.SimulationRun = _mapper.Map<SimulationRunWithIdDTO>(ss.Single(q => q.Id == item.Id).SimulationRun);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific simulation scenario by its ID.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to retrieve.</param>
        /// <returns>The simulation scenario.</returns>

        [HttpGet("/simulationScenario/{simulationScenarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenario>>> GetSimulationScenario(long simulationScenarioId)
        {
            var result = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId);
            if (result == null)
            {
                string error = "This simulation scenario does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Deletes a specific simulation scenario by its ID.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to delete.</param>
        /// <returns>Confirmation message upon successful deletion.</returns>

        [HttpDelete("/simulationScenario/{simulationScenarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenario>>> DeleteSimulationScenario(long simulationScenarioId)
        {
            var result = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId);
            string error = string.Empty;
            if (result == null)
            {
                error = "This simulation scenario does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }
            var(success,message) = _repositorySimulationScenario.Remove(result);
            if (!success)
            {
                _logger.LogError("Error while deleting Simulation Scenario: " + message);
                return BadRequest(error);
            }
            message = "Simulation scenario with id: " + simulationScenarioId + ", removed";
            _logger.LogInformation(message);
            return Ok(message);

        }

        /// <summary>
        /// Deletes a specific simulation scenario by its ID, including cascading deletion of associated data.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to delete.</param>
        /// <returns>Confirmation message upon successful deletion.</returns>

        [HttpDelete("/simulationScenario/{simulationScenarioId}/cascade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenario>>> DeleteCascadeSimulationScenario(long simulationScenarioId)
        {
            var result = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId, include: s => s.Include(s => s.Population));
            string error = string.Empty;
            if (result == null)
            {
                error = "This simulation scenario does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }
            var (success, message) = _repositoryPopulation.Remove(result.Population);
            if (!success)
            {
                _logger.LogError("Error while deleting Simulation Scenario: " + message);
                return BadRequest(error);
            }
            message = "Simulation scenario with id: " + simulationScenarioId + ", removed in cascade";
            _logger.LogInformation(message);
            return Ok(message);

        }

        /// <summary>
        /// Deletes all simulation scenarios.
        /// </summary>
        /// <returns>Confirmation message upon successful deletion.</returns>

        [HttpDelete("/simulationScenario/delete")]
        public async Task<IActionResult> DeleteAllSimulationScenarios()
        {
            try
            {
                await _repositorySimulationScenario.DeleteAllAsync();
                string message = "All simulation scenarios have been deleted";
                _logger.LogInformation(message);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting Simulation Scenarios: "+ ex.Message);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Updates a specific simulation scenario by its ID.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to update.</param>
        /// <param name="simulationScenario">Updated simulation scenario data.</param>
        /// <returns>The updated simulation scenario.</returns>

        [HttpPut("/simulationScenario/{simulationScenarioId}/edit")]
        public async Task<ActionResult> UpdateSimulationScenario(SimulationScenario simulationScenario, long simulationScenarioId)
        {
            var result = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId);
            string error = string.Empty;
            if (result == null)
            {
                error = "This simulation scenario does not exist";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            result.LongTermModelBranch = simulationScenario.LongTermModelBranch;
            result.ShortTermModelBranch = simulationScenario.ShortTermModelBranch;
            result.Horizon = simulationScenario.Horizon;
            var (success, message) = _repositorySimulationScenario.Update(result);
            if (!success)
            {
                _logger.LogError("Error while updating Simulation Scenario: " + message);
                return BadRequest(error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all simulation scenarios.
        /// </summary>
        /// <returns>List of simulation scenarios.</returns>

        [HttpGet("/simulationScenarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationScenarioWithIdDTO>>> GetSimulationScenarios()
        {
            var result = await _repositorySimulationScenario.GetAllAsync();
            
            if (result == null)
            {
                _logger.LogInformation("Ther are no SimulationScenarios in the database");
                return NoContent();
            }
            var resultDTO = _mapper.Map<List<SimulationScenarioWithIdDTO>>(result);
            return Ok(resultDTO);
        }
    }
}
