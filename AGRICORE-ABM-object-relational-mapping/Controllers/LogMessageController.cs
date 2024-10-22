using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing log messages related to simulation runs.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LogMessageController : ControllerBase
    {

        private readonly IRepository<LogMessage> _repositoryLogMessage;
        private readonly IRepository<SimulationRun> _repositorySimulationRun;
        private readonly ILogger<LogMessageController> _logger;

        public LogMessageController(
            IRepository<LogMessage> repositoryLogMessage,
            IRepository<SimulationRun> repositorySimulationRun,
            ILogger<LogMessageController> logger
        )
        {
            _repositoryLogMessage = repositoryLogMessage;
            _repositorySimulationRun = repositorySimulationRun;
            _logger = logger;
        }

        /// <summary>
        /// Adds a log message to a specific simulation run.
        /// </summary>
        /// <param name="simulationRunId">Simulation run ID.</param>
        /// <param name="logMessage">Log message to add.</param>
        /// <returns>Created log message.</returns>
        [HttpPost("/simulationRun/{simulationRunId}/logMessage/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LogMessage>> AddLogMessage(long simulationRunId, LogMessage logMessage)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: "+ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }
            string error = string.Empty;
            var existingSimulation = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId);

            if (existingSimulation == null)
            {
                error = "There is no simulation run with this id: " + simulationRunId;
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            logMessage.SimulationRunId = simulationRunId;
            var(success,message) = await _repositoryLogMessage.AddAsync(logMessage);
            if (success)
            {
                _logger.LogInformation($"LogMessage {logMessage.Id} added");
                return CreatedAtAction(nameof(AddLogMessage), new { id = logMessage.Id }, logMessage);
            }
            else
            {
                _logger.LogError("Error while inserting LogMessage: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds a range of log messages to a specific simulation run.
        /// </summary>
        /// <param name="simulationRunId">Simulation run ID.</param>
        /// <param name="logMessages">List of log messages to add.</param>
        /// <returns>Created log messages.</returns>
        [HttpPost("/simulationRun/{simulationRunId}/logMessage/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LogMessage>> AddLogMessageRange(long simulationRunId, List<LogMessage> logMessages)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state: " + ErrorHelper.GetErrorDescription(ModelState));
                return BadRequest(ModelState);
            }

            var existingSimulation = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId);
            string error = string.Empty;
            if (existingSimulation == null)
            {
                error = "There is no simulation run with this id: " + simulationRunId;
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            logMessages.ForEach(l => l.SimulationRunId = simulationRunId);

            var(success,message) = await _repositoryLogMessage.AddRangeAsync(logMessages);
            if (success)
            {
                var createdLogMessacgesId = logMessages.Select(l => l.Id).ToList();
                var result = new { ids = createdLogMessacgesId };
                _logger.LogInformation($"LogMessage {String.Join(",",createdLogMessacgesId)} added");
                return CreatedAtAction(nameof(AddLogMessageRange), new { result }, logMessages);
            }
            else
            {
                _logger.LogError("Error while inserting LogMessage: " + message);
                return BadRequest(message);
            }

            
        }

        /// <summary>
        /// Retrieves all log messages associated with a specific simulation run.
        /// </summary>
        /// <param name="simulationRunId">Simulation run ID.</param>
        /// <returns>List of log messages.</returns>
        [HttpGet("/simulationRun/{simulationRunId}/logMessage/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<LogMessage>>> GetAllLogMessages(long simulationRunId)
        {
            var existingSimulation = await _repositorySimulationRun.GetSingleOrDefaultAsync(s => s.Id == simulationRunId, include: s => s.Include(s => s.LogMessages));
            string error = string.Empty;
            if (existingSimulation == null)
            {
                error = "There is no simulation run with this id: " + simulationRunId;
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            if (existingSimulation.LogMessages == null || existingSimulation.LogMessages.Count == 0)
            {
                error = "There are no log messages in this simulation run";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(existingSimulation.LogMessages);
        }

        /// <summary>
        /// Retrieves a specific log message by its ID.
        /// </summary>
        /// <param name="logMessageId">Log message ID.</param>
        /// <returns>Log message.</returns>
        [HttpGet("/logMessage/{logMessageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<LogMessage>>> GetLogMessage(long logMessageId)
        {
            var result = await _repositoryLogMessage.GetSingleOrDefaultAsync(l => l.Id == logMessageId);
            string error = string.Empty;
            if (result == null)
            {
                error = "This log message does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }



    }
}
