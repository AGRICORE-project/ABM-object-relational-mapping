using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a JSON data transfer object (DTO) for a population, 
    /// including farms, product groups, policies, and land transactions.
    /// </summary>
    public class PopulationJsonDTO
    {
        /// <summary>
        /// Gets or sets the description of the population.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of farms associated with the population.
        /// </summary>
        public List<FarmJsonDTO> Farms { get; set; } = new List<FarmJsonDTO>();

        /// <summary>
        /// Gets or sets the list of product groups associated with the population.
        /// </summary>
        public List<ProductGroupJsonDTO> ProductGroups { get; set; } = new List<ProductGroupJsonDTO>();

        /// <summary>
        /// Gets or sets the list of policies associated with the population.
        /// </summary>
        public List<PolicyJsonDTO> Policies { get; set; } = new List<PolicyJsonDTO>();

        /// <summary>
        /// Gets or sets the list of policy group relations associated with the population.
        /// </summary>
        public List<PolicyGroupRelationJsonDTO> PolicyGroupRelations { get; set; } = new List<PolicyGroupRelationJsonDTO>();

        /// <summary>
        /// Gets or sets the list of land transactions associated with the population.
        /// </summary>
        public List<LandTransactionJsonDTO> LandTransactions { get; set; }

        /// <summary>
        /// Gets or sets the list of land rents associated with the population.
        /// </summary>
        public List<LandRentJsonDTO> LandRents { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object (DTO) for the creation of a new population.
    /// </summary>
    public class PopulationCreationDTO
    {
        /// <summary>
        /// Gets or sets the description of the population being created.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
