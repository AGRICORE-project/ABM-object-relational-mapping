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
    /// Represents holder data for a specific farm and year.
    /// </summary>
    [Index(nameof(FarmId), nameof(YearId), IsUnique = true)]
    public class HolderFarmYearData : Entity
    {
        /// <summary>
        /// The farm Id associated with this holder data.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// The farm associated with this holder data.
        /// </summary>
        [JsonIgnore]
        public Farm Farm { get; set; } = null!;

        /// <summary>
        /// The year Id associated with this holder data.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// The year associated with this holder data.
        /// </summary>
        [JsonIgnore]
        public Year Year { get; set; } = null!;

        /// <summary>
        /// The age of the farm holder.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public int HolderAge { get; set; }

        /// <summary>
        /// The number of family members of the farm holder.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public int HolderFamilyMembers { get; set; }

        /// <summary>
        /// The age of the successors of the farm holder.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public int HolderSuccessorsAge { get; set; }

        /// <summary>
        /// The gender of the farm holder.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public GenderEnum HolderGender { get; set; }

        /// <summary>
        /// The number of successors of the farm holder.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long HolderSuccessors { get; set; }
    }

    /// <summary>
    /// Represents the gender of the farm holder.
    /// </summary>
    public enum GenderEnum
    {
        Male = 1,
        Female = 2
    }
}
