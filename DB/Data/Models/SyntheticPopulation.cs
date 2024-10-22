using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a synthetic population, which is a modeled version of a real population.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    public class SyntheticPopulation : Entity
    {
        /// <summary>
        /// Gets or sets the description of the synthetic population.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the synthetic population.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the population to which this synthetic population belongs.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the population to which this synthetic population belongs.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the year associated with this synthetic population.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the year associated with this synthetic population.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; }
    }
}
