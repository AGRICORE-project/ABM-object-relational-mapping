using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents FADN product data.
    /// </summary>
    public class FADNProductJsonDTO
    {
        /// <summary>
        /// Gets or sets the FADN identifier.
        /// </summary>
        public string? FADNIdentifier { get; set; }
        
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the product type.
        /// </summary>
        public string? ProductType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is arable.
        /// </summary>
        public bool Arable { get; set; }

        /// <summary>
        /// Gets or sets the representativeness occurrence.
        /// </summary>
        // The representativeness of the FADN product in the product group. Item representativeness should be calculated
        // as the representativeness of the FADN product divided by the sum of the representativeness of all FADN products in the product group.
        // For its calculation, remember than whe obtaining data from the sample, the weight of such farm in the population should be taken into account
        public float RepresentativenessOcurrence { get; set; } = 0;

        /// <summary>
        /// Gets or sets the representativeness area.
        /// </summary>
        public float RepresentativenessArea { get; set; } = 0;

        /// <summary>
        /// Gets or sets the representativeness value.
        /// </summary>
        public float RepresentativenessValue { get; set; } = 0;
    }
}