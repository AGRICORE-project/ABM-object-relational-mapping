using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a single execution of a simulation scenario.
    /// Contains details about the current status and progress of the simulation.
    /// </summary>
    public class SimulationRun : Entity
    {
        /// <summary>
        /// Gets or sets the identifier of the simulation scenario associated with this simulation run.
        /// </summary>
        public long SimulationScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the simulation scenario associated with this simulation run.
        /// </summary>
        [JsonIgnore]
        public SimulationScenario? SimulationScenario { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of log messages associated with this simulation run.
        /// </summary>
        [JsonIgnore]
        public List<LogMessage>? LogMessages { get; set; } = null!;

        /// <summary>
        /// Gets or sets the overall status of the simulation run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public OverallStatus OverallStatus { get; set; }

        /// <summary>
        /// Gets or sets the current stage of the simulation run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public SimulationStage CurrentStage { get; set; }

        /// <summary>
        /// Gets or sets the current year of the simulation run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public int CurrentYear { get; set; }

        /// <summary>
        /// Gets or sets the current substage of the simulation run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string CurrentSubstage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the progress of the current stage of the simulation run, represented as a percentage.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int CurrentStageProgress { get; set; }

        /// <summary>
        /// Gets or sets the progress of the current substage of the simulation run, represented as a percentage.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int CurrentSubStageProgress { get; set; }
    }

    /// <summary>
    /// Defines the overall status of a simulation run.
    /// </summary>
    public enum OverallStatus
    {
        /// <summary>
        /// The simulation run is in progress.
        /// </summary>
        INPROGRESS = 1,

        /// <summary>
        /// The simulation run was cancelled.
        /// </summary>
        CANCELLED = 2,

        /// <summary>
        /// The simulation run was completed successfully.
        /// </summary>
        COMPLETED = 3,

        /// <summary>
        /// The simulation run encountered an error.
        /// </summary>
        ERROR = 4,
    }

    /// <summary>
    /// Defines the stages of a simulation run.
    /// </summary>
    public enum SimulationStage
    {
        /// <summary>
        /// The data preparation stage.
        /// </summary>
        DATAPREPARATION = 1,

        /// <summary>
        /// The long period stage.
        /// </summary>
        LONGPERIOD = 2,

        /// <summary>
        /// The land market stage.
        /// </summary>
        LANDMARKET = 3,

        /// <summary>
        /// The short period stage.
        /// </summary>
        SHORTPERIOD = 4,

        /// <summary>
        /// The realisation stage.
        /// </summary>
        REALISATION = 5,
    }
}
