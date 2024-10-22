using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a population with a description and various relationships.
    /// </summary>
    public class Population : Entity
    {
        /// <summary>
        /// The description of the population.
        /// </summary>
        [NotNull]
        public string Description { get; set; } = string.Empty;

        #region Relationships
        /// <summary>
        /// A list of farms associated with the population.
        /// </summary>
        [JsonIgnore]
        public List<Farm>? Farms { get; set; }

        /// <summary>
        /// A list of years associated with the population.
        /// </summary>
        [JsonIgnore]
        public List<Year>? Years { get; set; }

        /// <summary>
        /// A list of FADN product relations.
        /// </summary>
        [JsonIgnore]
        public List<FADNProductRelation>? FADNProductRelations { get; set; }

        /// <summary>
        /// A list of product groups associated with the population.
        /// </summary>
        [JsonIgnore]
        public List<ProductGroup>? ProductGroups { get; set; }

        /// <summary>
        /// A list of policy group relations.
        /// </summary>
        [JsonIgnore]
        public List<PolicyGroupRelation>? PolicyGroupRelations { get; set; }

        /// <summary>
        /// A list of synthetic populations.
        /// </summary>
        [JsonIgnore]
        public List<SyntheticPopulation>? SyntheticPopulations { get; set; }

        /// <summary>
        /// A list of simulation scenarios.
        /// </summary>
        [JsonIgnore]
        public List<SimulationScenario>? SimulationScenarios { get; set; }

        /// <summary>
        /// A list of policies associated with the population.
        /// </summary>
        [JsonIgnore]
        public List<Policy> Policies { get; set; }
        #endregion
    }
}
