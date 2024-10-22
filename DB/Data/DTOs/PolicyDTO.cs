using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a JSON data transfer object (DTO) for a policy, 
    /// including its identification, description, and economic compensation details.
    /// </summary>
    public class PolicyJsonDTO
    {
        /// <summary>
        /// Gets or sets the population identifier associated with the policy.
        /// </summary>
        public long PopulationId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the unique identifier for the policy.
        /// </summary>
        public string? PolicyIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the policy is coupled.
        /// </summary>
        public Boolean IsCoupled { get; set; }

        /// <summary>
        /// Gets or sets the description of the policy.
        /// </summary>
        public string? PolicyDescription { get; set; }

        /// <summary>
        /// Gets or sets the economic compensation for the policy.
        /// For the coupled policies, this value is a rate to be multiplied by the hectares of the associated crops.
        /// The compensation is weighted in relation to the original distribution of the crops in the original population.
        /// </summary>
        // Economic compensation for the policy. For the coupled ones, this value is a rate to be multiplied by the ha of the associated crops
        // The compensation is weighted in relation with the original distribution of the crops in the original population
        public float EconomicCompensation { get; set; } = 0;

        /// <summary>
        /// Gets or sets the model label associated with the policy.
        /// </summary>
        public string? ModelLabel { get; set; }

        /// <summary>
        /// Gets or sets the starting year number for the policy.
        /// </summary>
        public int StartYearNumber { get; set; }

        /// <summary>
        /// Gets or sets the ending year number for the policy.
        /// </summary>
        public int EndYearNumber { get; set; }
    }
}
