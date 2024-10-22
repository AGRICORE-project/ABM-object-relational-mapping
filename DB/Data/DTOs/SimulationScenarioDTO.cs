using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DB.Data.DTOs
{
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for adding a new simulation scenario,
    /// including settings for model branches, policies, and other parameters.
    /// </summary>
    public class SimulationScenarioAddDTO
    {
        /// <summary>
        /// Gets or sets the identifier for the synthetic population used in the simulation scenario.
        /// </summary>
        public long SyntheticPopulationId { get; set; }

        /// <summary>
        /// Gets or sets the branch of the short-term model used in the simulation scenario.
        /// </summary>
        public string ShortTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch of the long-term model used in the simulation scenario.
        /// </summary>
        public string LongTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore linear programming (LP) in the simulation scenario.
        /// </summary>
        public bool IgnoreLP { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore land market model (LMM) in the simulation scenario.
        /// </summary>
        public bool IgnoreLMM { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to compress the simulation scenario data.
        /// </summary>
        public bool Compress { get; set; } = false;

        /// <summary>
        /// Gets or sets the horizon (duration) of the simulation scenario in years.
        /// </summary>
        public int Horizon { get; set; }

        /// <summary>
        /// Gets or sets the suffix used for queueing the simulation scenario.
        /// </summary>
        public string QueueSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of additional policies to be applied in the simulation scenario.
        /// </summary>
        public List<PolicyForUIDTO>? AdditionalPolicies { get; set; } = new List<PolicyForUIDTO>();
    }
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for a simulation scenario, 
    /// including its identification, model branches, and policies.
    /// </summary>
    public class SimulationScenarioWithIdDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the simulation scenario.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the population identifier associated with the simulation scenario.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the year identifier associated with the simulation scenario.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore linear programming (LP) in the simulation scenario.
        /// </summary>
        public bool IgnoreLP { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore land market model (LMM) in the simulation scenario.
        /// </summary>
        public bool IgnoreLMM { get; set; } = false;

        /// <summary>
        /// Gets or sets the branch of the short-term model used in the simulation scenario.
        /// </summary>
        public string ShortTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch of the long-term model used in the simulation scenario.
        /// </summary>
        public string LongTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the horizon (duration) of the simulation scenario in years.
        /// </summary>
        public int Horizon { get; set; }

        /// <summary>
        /// Gets or sets the suffix used for queueing the simulation scenario.
        /// </summary>
        public string QueueSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of additional policies to be applied in the simulation scenario.
        /// </summary>
        public List<PolicyForUIDTO>? AdditionalPolicies { get; set; } = new List<PolicyForUIDTO>();
    }
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for a simulation scenario with an associated simulation run,
    /// including details about the model branches, policies, and current progress.
    /// </summary>
    public class SimulationScenarioWithScenarioRunDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the simulation scenario.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the population identifier associated with the simulation scenario.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the year identifier associated with the simulation scenario.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore linear programming (LP) in the simulation scenario.
        /// </summary>
        public bool IgnoreLP { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore land market model (LMM) in the simulation scenario.
        /// </summary>
        public bool IgnoreLMM { get; set; } = false;

        /// <summary>
        /// Gets or sets the branch of the short-term model used in the simulation scenario.
        /// </summary>
        public string ShortTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch of the long-term model used in the simulation scenario.
        /// </summary>
        public string LongTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the horizon (duration) of the simulation scenario in years.
        /// </summary>
        public int Horizon { get; set; }

        /// <summary>
        /// Gets or sets the suffix used for queueing the simulation scenario.
        /// </summary>
        public string QueueSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of additional policies to be applied in the simulation scenario.
        /// </summary>
        public List<PolicyForUIDTO>? AdditionalPolicies { get; set; } = new List<PolicyForUIDTO>();

        /// <summary>
        /// Gets or sets the simulation run details associated with the simulation scenario.
        /// </summary>
        public SimulationRunWithIdDTO SimulationRun { get; set; }
    }
}