using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DB.Data.DTOs
{
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for a simulation run, 
    /// including its status, current stage, and progress.
    /// </summary>
    public class SimulationRunWithIdDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the simulation run.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the associated simulation scenario.
        /// </summary>
        public long SimulationScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the overall status of the simulation run.
        /// </summary>
        public OverallStatus OverallStatus { get; set; }

        /// <summary>
        /// Gets or sets the current stage of the simulation run.
        /// </summary>
        public SimulationStage CurrentStage { get; set; }

        /// <summary>
        /// Gets or sets the current year in the simulation run.
        /// </summary>
        public int CurrentYear { get; set; }

        /// <summary>
        /// Gets or sets the current substage of the simulation run.
        /// </summary>
        public string CurrentSubstage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the progress of the current stage in the simulation run.
        /// </summary>
        public int CurrentStageProgress { get; set; }

        /// <summary>
        /// Gets or sets the progress of the current substage in the simulation run.
        /// </summary>
        public int CurrentSubStageProgress { get; set; }
    }
}