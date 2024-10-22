using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    [Index(nameof(PopulationId), nameof(PolicyIdentifier), IsUnique = true)]
    /// <summary>
    /// Represents a policy with a unique identifier, coupling status, population ID, and related information.
    /// </summary>
    public class Policy : Entity
    {
        /// <summary>
        /// The unique identifier for the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string? PolicyIdentifier { get; set; }

        /// <summary>
        /// Indicates if the policy is coupled.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public Boolean IsCoupled { get; set; }

        /// <summary>
        /// The population ID associated with the policy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public long PopulationId { get; set; }

        /// <summary>
        /// The population associated with the policy.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// The description of the policy.
        /// </summary>
        [Required]
        public string? PolicyDescription { get; set; }

        /// <summary>
        /// A list of policy group relations.
        /// </summary>
        public List<PolicyGroupRelation>? PolicyGroupRelations { get; set; }

        /// <summary>
        /// The economic compensation for the policy.
        /// </summary>
        // This is used only if there is no policy group relation
        public float EconomicCompensation { get; set; } = 0;

        /// <summary>
        /// The model label for the policy.
        /// </summary>
        public string? ModelLabel { get; set; }

        /// <summary>
        /// The start year number for the policy.
        /// </summary>
        public int StartYearNumber { get; set; } = 0;

        /// <summary>
        /// The end year number for the policy.
        /// </summary>
        public int EndYearNumber { get; set; } = 0;
    }

    [PrimaryKey(nameof(Id))]
    [Index(nameof(ProductGroupId), nameof(PolicyId), nameof(PopulationId), IsUnique = true)]
    /// <summary>
    /// Represents the relation between a policy group and a population, including economic compensation.
    /// </summary>
    public class PolicyGroupRelation : Entity
    {
        /// <summary>
        /// The product group ID associated with the policy group relation.
        /// </summary>
        public long ProductGroupId { get; set; }

        /// <summary>
        /// The product group associated with the policy group relation.
        /// </summary>
        [JsonIgnore]
        public ProductGroup? ProductGroup { get; set; } = null!;

        /// <summary>
        /// The policy ID associated with the policy group relation.
        /// </summary>
        public long PolicyId { get; set; }

        /// <summary>
        /// The policy associated with the policy group relation.
        /// </summary>
        [JsonIgnore]
        public Policy? Policy { get; set; } = null!;

        /// <summary>
        /// The population ID associated with the policy group relation.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// The population associated with the policy group relation.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// The economic compensation for the policy, weighted by the original crop distribution.
        /// </summary>
        // Economic compensation for the policy. For the coupled ones, this values is a rate to be multiplied by the ha of the associated crops
        // The compensation is weighted in relation with the original distribution of the crops in the original population
        [JsonProperty(Required = Required.Always)]
        [Required]
        public float EconomicCompensation { get; set; }
    }

}
