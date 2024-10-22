using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a JSON data transfer object (DTO) for the relationship between policies and product groups,
    /// including economic compensation details.
    /// </summary>
    public class PolicyGroupRelationJsonDTO
    {
        /// <summary>
        /// Gets or sets the population identifier associated with the policy group relation.
        /// </summary>
        public long PopulationId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the unique identifier for the policy.
        /// </summary>
        public string PolicyIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the product group associated with the policy.
        /// </summary>
        public string ProductGroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the economic compensation associated with the policy and product group.
        /// </summary>
        public float EconomicCompensation { get; set; } = 0;
    }
}
