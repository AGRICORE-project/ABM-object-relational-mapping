using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DB.Data.Models
{
    [Index(nameof(OriginFarmId), nameof(DestinationFarmId), nameof(YearId), IsUnique = true)]
    /// <summary>
    /// Represents a land rent transaction between farms within a specific year.
    /// </summary>
    public class LandRent : Entity
    {
        /// <summary>
        /// The ID of the originating farm.
        /// </summary>
        [Required]
        public long OriginFarmId { get; set; }

        /// <summary>
        /// The ID of the destination farm.
        /// </summary>
        [JsonIgnore]
        [Required]
        public long DestinationFarmId { get; set; }

        /// <summary>
        /// The ID of the year in which the land rent transaction takes place.
        /// </summary>
        [JsonIgnore]
        [Required]
        public long YearId { get; set; }
        // Total Rent Price [€]
        /// <summary>
        /// The total rent value in euros.
        /// </summary>
        public float RentValue { get; set; }
        // Total Rent Area [ha]
        /// <summary>
        /// The total area rented in hectares.
        /// </summary>
        public float RentArea { get; set; }

        #region Relationships
        /// <summary>
        /// The originating farm entity.
        /// </summary>
        [JsonIgnore]
        public Farm? OriginFarm { get; set; }

        /// <summary>
        /// The destination farm entity.
        /// </summary>
        [JsonIgnore]
        public Farm? DestinationFarm { get; set; }

        /// <summary>
        /// The year entity.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; }
        #endregion
    }
}
