using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a subsidy associated with a specific farm and year.
    /// </summary>
    [Index(nameof(FarmId), nameof(YearId), nameof(PolicyId), IsUnique = true)]
    public class FarmYearSubsidy : Entity
    {
        /// <summary>
        /// The value of the subsidy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Column("VALUE")]
        [Required]
        public float Value { get; set; }

        /// <summary>
        /// The farm Id associated with this subsidy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long FarmId { get; set; }

        /// <summary>
        /// The farm associated with this subsidy.
        /// </summary>
        [JsonIgnore]
        public Farm? Farm { get; set; } = null!;

        /// <summary>
        /// The year Id associated with this subsidy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long YearId { get; set; }

        /// <summary>
        /// The year associated with this subsidy.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; } = null!;

        /// <summary>
        /// The policy Id associated with this subsidy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long PolicyId { get; set; }

        /// <summary>
        /// The policy associated with this subsidy.
        /// </summary>
        [JsonIgnore]
        public Policy? Policy { get; set; } = null!;
    }
}
