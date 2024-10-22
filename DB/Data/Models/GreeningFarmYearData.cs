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
    /// Represents greening data for a specific farm and year.
    /// </summary>
    [Index(nameof(FarmId), nameof(YearId), IsUnique = true)]
    public class GreeningFarmYearData : Entity
    {
        /// <summary>
        /// The farm Id associated with this greening data.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// The farm associated with this greening data.
        /// </summary>
        [JsonIgnore]
        public Farm Farm { get; set; } = null!;

        /// <summary>
        /// The year Id associated with this greening data.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// The year associated with this greening data.
        /// </summary>
        [JsonIgnore]
        public Year Year { get; set; } = null!;

        /// <summary>
        /// The amount of greening surface in hectares.
        /// </summary>
        // Amount of greening surface in hectares
        [JsonProperty(Required = Required.Always)]
        [Required]
        public float GreeningSurface { get; set; }
    }
}
