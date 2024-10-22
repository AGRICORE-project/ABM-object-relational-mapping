using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.DTOs
{
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for coupled compensation information displayed in the UI.
    /// </summary>
    public class CoupledCompensationForUIDTO
    {
        /// <summary>
        /// Gets or sets the product group associated with the compensation.
        /// </summary>
        public string? ProductGroup { get; set; }

        /// <summary>
        /// Gets or sets the economic compensation for the coupled policy.
        /// </summary>
        public float EconomicCompensation { get; set; }
    }

    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for policy information displayed in the UI, 
    /// including related coupled compensations.
    /// </summary>
    public class PolicyForUIDTO
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
        /// </summary>
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

        /// <summary>
        /// Gets or sets the list of coupled compensations associated with the policy.
        /// </summary>
        public List<CoupledCompensationForUIDTO>? CoupledCompensations { get; set; } = new List<CoupledCompensationForUIDTO>();
    }
}
