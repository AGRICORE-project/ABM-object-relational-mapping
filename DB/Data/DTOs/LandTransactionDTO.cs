using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Newtonsoft.Json;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a data transfer object (DTO) for land transactions between farms.
    /// </summary>
    [NotMapped]
    public class LandTransactionDTO
    {
        /// <summary>
        /// Gets or sets the ID of the production associated with the land transaction.
        /// </summary>
        public long ProductionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the destination farm where the land is transferred.
        /// </summary>
        public long DestinationFarmId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the year associated with the land transaction.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the land transferred from the origin farm to the destination farm.
        /// This value should be between 0 and 1.
        /// </summary>
        [Range(0, 1, ErrorMessage = "Percentage should be saved as a value between 0 and 1")]
        // Percentage of the land transferred from the origin farm to the destination farm in [0,1] range
        public float Percentage { get; set; }

        /// <summary>
        /// Gets or sets the sale price of the land transferred from the origin farm to the destination farm in Euros.
        /// </summary>
        // Sale price of the land transferred from the origin farm to the destination farm [€]
        public float SalePrice { get; set; }
    }

    /// <summary>
    /// Represents a JSON data transfer object (DTO) for land transactions between farms, 
    /// including details such as year, product group, and farm codes.
    /// </summary>
    [NotMapped]
    public class LandTransactionJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number associated with the land transaction.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the product group associated with the land transaction.
        /// </summary>
        public string? ProductGroupName { get; set; }

        /// <summary>
        /// Gets or sets the code of the destination farm where the land is transferred.
        /// </summary>
        public string? DestinationFarmCode { get; set; }

        /// <summary>
        /// Gets or sets the code of the origin farm from where the land is transferred.
        /// </summary>
        public string? OriginFarmCode { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the land transferred from the origin farm to the destination farm.
        /// This value should be between 0 and 1.
        /// </summary>
        [Range(0, 1, ErrorMessage = "Percentage should be saved as a value between 0 and 1")]
        // Percentage of the land transferred from the origin farm to the destination farm in [0,1] range
        public float Percentage { get; set; }

        /// <summary>
        /// Gets or sets the sale price of the land transferred from the origin farm to the destination farm in Euros.
        /// </summary>
        // Sale price of the land transferred from the origin farm to the destination farm [€]
        public float SalePrice { get; set; }
    }
}
