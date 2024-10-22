using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;
namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing simulation runs and scenarios.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SimulationRunController : ControllerBase
    {
        private readonly IRepository<SimulationRun> _repositorySimulationRun;
        private readonly IRepository<SimulationScenario> _repositorySimulationScenario;
        private readonly ILogger _logger;

        public SimulationRunController(
            IRepository<SimulationRun> repositorySimulationRun,
            IRepository<SimulationScenario> repositorySimulationScenario,
            ILogger<SimulationRunController> logger
        )
        {
            _repositorySimulationRun = repositorySimulationRun;
            _repositorySimulationScenario = repositorySimulationScenario;
            _logger = logger;
        }

        /// <summary>
        /// Adds a simulation run to a specified simulation scenario.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to add the run to.</param>
        /// <param name="simulationRun">Simulation run data to add.</param>
        /// <returns>The added simulation run.</returns>

        [HttpPost("/simulationScenario/{simulationScenarioId}/simulationRun/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SimulationRun>> AddSimulationRun(long simulationScenarioId, SimulationRun simulationRun)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invallid model state: "+ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }

            var existingSimulationScenario = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId, include: s => s.Include(s => s.SimulationRun));
            string error = string.Empty;
            if (existingSimulationScenario == null)
            {
                error = "There is no simulation scenario with this id: " + simulationRun.SimulationScenarioId;
                _logger.LogError(error);
                return StatusCode(404, error);
            };

            if (existingSimulationScenario.SimulationRun != null)
            {
                error = $@"A simulation run for simulation scenario {simulationRun.SimulationScenarioId} already exist";
                _logger.LogError(error);
                return BadRequest(error);
            }
            simulationRun.SimulationScenarioId = simulationScenarioId;

            var(success,message) = await _repositorySimulationRun.AddAsync(simulationRun);
            if (success)
            {
                _logger.LogInformation($"SimulationRun {simulationRun.Id} added");
                return CreatedAtAction(nameof(AddSimulationRun), new { id = simulationRun.Id }, simulationRun);
            }
            else{ 
                _logger.LogError("Error while inserting SimulationRun: "+message); 
                return BadRequest(message);
            }
            
        }

        /// <summary>
        /// Adds a range of simulation runs to their respective simulation scenarios.
        /// </summary>
        /// <param name="simulationRuns">List of simulation runs to add.</param>
        /// <returns>The added simulation runs.</returns>

        [HttpPost("/simulationRun/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SimulationRun>> AddSimulationRunRange(List<SimulationRun> simulationRuns)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invallid model state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            string error = string.Empty;
            foreach (SimulationRun sim in simulationRuns)
            {
                var existingSimulationScenario = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == sim.SimulationScenarioId, include: s => s.Include(s => s.SimulationRun));

                if (existingSimulationScenario == null)
                {
                    error = "There is no simulation scenario with this id: " + sim.SimulationScenarioId;
                    _logger.LogError(error);
                    return StatusCode(404, error);
                }

                if (existingSimulationScenario.SimulationRun != null)
                {
                    error = $@"A simulation run for simulation scenario {sim.SimulationScenarioId} already exist";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
            }

            var (success, message) = await _repositorySimulationRun.AddRangeAsync(simulationRuns);
            if (success)
            {
                var createdSimulationRunsId = simulationRuns.Select(s => s.Id).ToList();
                var result = new { ids = createdSimulationRunsId };
                _logger.LogInformation($"SimulationRun {String.Join(",",createdSimulationRunsId)} added");
                return CreatedAtAction(nameof(AddSimulationRunRange), new { result }, simulationRuns);
            }
            else
            {
                _logger.LogError("Error while inserting SimulationRun: " + message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all simulation runs associated with a specific simulation scenario.
        /// </summary>
        /// <param name="simulationScenarioId">ID of the simulation scenario to retrieve runs from.</param>
        /// <returns>List of simulation runs.</returns>

        [HttpGet("/simulationScenario/{simulationScenarioId}/simulationRun/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationRun>>> GetAllSimulationRunsFromScenario(long simulationScenarioId)
        {
            var existingSimulationScenario = await _repositorySimulationScenario.GetSingleOrDefaultAsync(s => s.Id == simulationScenarioId, include: s => s.Include(s => s.SimulationRun));
            string error = string.Empty;
            if (existingSimulationScenario == null)
            {
                error = "There is no simulation scenario with this id: " + simulationScenarioId;
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }


            if (existingSimulationScenario.SimulationRun == null)
            {
                error = "There is no simulation run for this simulation scenario";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(existingSimulationScenario.SimulationRun);
        }

        /// <summary>
        /// Retrieves a specific simulation run by its ID.
        /// </summary>
        /// <param name="simulationRunId">ID of the simulation run to retrieve.</param>
        /// <returns>The simulation run.</returns>

        [HttpGet("/simulationRun/{simulationRunId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationRun>>> GetSimulationRun(long simulationRunId)
        {
            var result = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId);
            if (result == null)
            {
                string error = "This simulation run does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Deletes a specific simulation run by its ID.
        /// </summary>
        /// <param name="simulationRunId">ID of the simulation run to delete.</param>
        /// <returns>Confirmation message upon successful deletion.</returns>

        [HttpDelete("/simulationRun/{simulationRunId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<SimulationRun>>> DeleteSimulationRun(long simulationRunId)
        {
            var result = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId);
            string error = string.Empty;
            if (result == null)
            {
                error = "This simulation run does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            var(success,message) = _repositorySimulationRun.Remove(result);
            if (success)
            {
                string successMessage = "Simulation run with id: " + simulationRunId + ", removed";
                _logger.LogInformation(successMessage);
                return Ok(successMessage);
            }
            else
            {
                error = $"Error deleting Simulation run {simulationRunId}";
                return BadRequest(error);
            }

        }

        /// <summary>
        /// Deletes all simulation runs.
        /// </summary>
        /// <returns>Confirmation message upon successful deletion.</returns>

        [HttpDelete("/simulationRun/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAllSimulationRuns()
        {
            try
            {
                await _repositorySimulationRun.DeleteAllAsync();
                string message = "All simulation runs have been deleted";
                _logger.LogInformation(message);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting Simulation runs: "+ex.Message);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Updates a specific simulation run by its ID.
        /// </summary>
        /// <param name="simulationRunId">ID of the simulation run to update.</param>
        /// <param name="simulationRun">Updated simulation run data.</param>
        /// <returns>The updated simulation run.</returns>

        [HttpPut("/simulationRun/{simulationRunId}/edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateSimulationRun(long simulationRunId, SimulationRun simulationRun)
        {
            var result = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId);

            string error = string.Empty;
            if (result == null)
            {
                error = "This simulation run does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            result.CurrentSubStageProgress = simulationRun.CurrentSubStageProgress;
            result.CurrentStageProgress = simulationRun.CurrentStageProgress;
            result.CurrentStage = simulationRun.CurrentStage;
            result.OverallStatus = simulationRun.OverallStatus;
            result.CurrentSubstage = simulationRun.CurrentSubstage;
            result.CurrentYear = simulationRun.CurrentYear;

            var(success,message) = _repositorySimulationRun.Update(result);
            if(success) {
                _logger.LogInformation($"Simulation run {simulationRun.Id} updated");
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Error while updating {simulationRun.Id}: " + message);
                return BadRequest(message);
            }
            
        }


    }
}
