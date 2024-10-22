using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a scenario for a simulation, including configuration settings and options.
    /// </summary>
    public class SimulationScenario : Entity
    {
        /// <summary>
        /// Gets or sets the identifier of the population associated with this simulation scenario.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the population associated with this simulation scenario.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore long-period calculations.
        /// </summary>
        public bool? IgnoreLP { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore land market modeling.
        /// </summary>
        public bool? IgnoreLMM { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to compress the scenario data.
        /// </summary>
        public bool? Compress { get; set; } = false;

        /// <summary>
        /// Gets or sets the identifier of the year associated with this simulation scenario.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the year associated with this simulation scenario.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; } = null!;

        /// <summary>
        /// Gets or sets the simulation run associated with this simulation scenario.
        /// </summary>
        [JsonIgnore]
        public SimulationRun? SimulationRun { get; set; } = null!;

        /// <summary>
        /// Gets or sets the branch of the short-term model used in this scenario.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string ShortTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch of the long-term model used in this scenario.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string LongTermModelBranch { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the horizon for the simulation scenario.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public int Horizon { get; set; }

        /// <summary>
        /// Gets or sets additional policies in JSON format for the simulation scenario.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [Column(TypeName = "jsonb")]
        public string AdditionalPolicies { get; set; } = "{}";
    }
}
