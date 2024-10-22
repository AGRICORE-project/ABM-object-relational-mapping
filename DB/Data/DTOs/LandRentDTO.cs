using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Newtonsoft.Json;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents land rent data.
    /// </summary>
    [NotMapped]
    public class LandRentDTO
    {
        /// <summary>
        /// Gets or sets the origin farm ID.
        /// </summary>
        public long OriginFarmId { get; set; }

        /// <summary>
        /// Gets or sets the destination farm ID.
        /// </summary>
        public long DestinationFarmId { get; set; }

        /// <summary>
        /// Gets or sets the year ID.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the total rent price [€].
        /// </summary>
        // Total Rent Price [€]
        public float RentValue { get; set; }

        /// <summary>
        /// Gets or sets the total rent area [ha].
        /// </summary>
        // Total Rent Area [ha]
        public float RentArea { get; set; }
    }

    /// <summary>
    /// Represents land rent data in JSON format.
    /// </summary>
    [NotMapped]
    public class LandRentJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the origin farm code.
        /// </summary>
        public string? OriginFarmCode { get; set; }

        /// <summary>
        /// Gets or sets the destination farm code.
        /// </summary>
        public string? DestinationFarmCode { get; set; }

        /// <summary>
        /// Gets or sets the total rent price [€].
        /// </summary>
        // Total Rent Price [€]
        public float RentValue { get; set; }

        /// <summary>
        /// Gets or sets the total rent area [ha].
        /// </summary>
        // Total Rent Area [ha]
        public float RentArea { get; set; }
    }
}
