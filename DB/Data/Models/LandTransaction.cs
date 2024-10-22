using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DB.Data.Models
{
    [Index(nameof(DestinationFarmId), nameof(ProductionId), nameof(YearId), IsUnique = true)]
    [Index(nameof(ProductionId), IsUnique = false)]
    /// <summary>
    /// Represents a land transaction involving production, a destination farm, and a specific year.
    /// </summary>
    public class LandTransaction : Entity
    {
        /// <summary>
        /// The ID of the production involved in the transaction.
        /// </summary>
        [Required]
        public long ProductionId { get; set; }

        /// <summary>
        /// The ID of the destination farm.
        /// </summary>
        [JsonIgnore]
        [Required]
        public long DestinationFarmId { get; set; }

        /// <summary>
        /// The ID of the year in which the land transaction takes place.
        /// </summary>
        [JsonIgnore]
        [Required]
        public long YearId { get; set; }

        /// <summary>
        /// The percentage of the transaction.
        /// </summary>
        [Range(0, 1, ErrorMessage = "Percentage should be saved as a value between 0 and 1")]
        public float Percentage { get; set; }

        /// <summary>
        /// The sale price of the land transaction.
        /// </summary>
        public float SalePrice { get; set; }

        #region Relationships
        /// <summary>
        /// The destination farm entity.
        /// </summary>
        [JsonIgnore]
        public Farm? DestinationFarm { get; set; }

        /// <summary>
        /// The agricultural production entity.
        /// </summary>
        [JsonIgnore]
        public AgriculturalProduction? Production { get; set; }

        /// <summary>
        /// The year entity.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; }
        #endregion
    }

}
