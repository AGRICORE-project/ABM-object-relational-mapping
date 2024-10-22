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
    /// Represents a farm entity with geographical and production data.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    [Index(nameof(FarmCode), nameof(PopulationId), IsUnique = true)]
    [Index(nameof(FarmCode), nameof(PopulationId), nameof(RegionLevel3), IsUnique = true)]
    public class Farm : Entity
    {
        /// <summary>
        /// The latitude of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long Lat { get; set; }

        /// <summary>
        /// The longitude of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long Long { get; set; }

        /// <summary>
        /// The altitude category of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public AltitudeEnum Altitude { get; set; }

        /// <summary>
        /// The primary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string RegionLevel1 { get; set; }

        /// <summary>
        /// The name of the primary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public String? RegionLevel1Name { get; set; }

        /// <summary>
        /// The secondary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string RegionLevel2 { get; set; }

        /// <summary>
        /// The name of the secondary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public String? RegionLevel2Name { get; set; }

        /// <summary>
        /// The tertiary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public long RegionLevel3 { get; set; }

        /// <summary>
        /// The name of the tertiary region level of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public String? RegionLevel3Name { get; set; }

        /// <summary>
        /// The farm's unique code.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string? FarmCode { get; set; }

        /// <summary>
        /// The technical and economic orientation of the farm.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public int TechnicalEconomicOrientation { get; set; }

        /// <summary>
        /// A list of agricultural productions associated with the farm.
        /// </summary>
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public List<AgriculturalProduction>? AgriculturalProductions { get; set; } = null!;

        /// <summary>
        /// A list of livestock productions associated with the farm.
        /// </summary>
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public List<LivestockProduction>? LivestockProductions { get; set; } = null!;

        /// <summary>
        /// The population Id associated with the farm.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// The population associated with the farm.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; }

        /// <summary>
        /// A list of subsidies associated with the farm for a particular year.
        /// </summary>
        [JsonIgnore]
        public List<FarmYearSubsidy>? FarmYearSubsidies { get; set; } = null!;

        /// <summary>
        /// A list of holder data associated with the farm for a particular year.
        /// </summary>
        [JsonIgnore]
        public List<HolderFarmYearData>? HoldersFarmYearData { get; set; } = null!;

        /// <summary>
        /// A list of closing valuations associated with the farm.
        /// </summary>
        [JsonIgnore]
        public List<ClosingValFarmValue>? ClosingValFarmValues { get; set; } = null!;

        /// <summary>
        /// A list of land transactions associated with the farm.
        /// </summary>
        [JsonIgnore]
        public List<LandTransaction>? LandTransactions { get; set; } = null!;

        /// <summary>
        /// A list of management decisions associated with the farm.
        /// </summary>
        [JsonIgnore]
        public List<AgroManagementDecision>? AgroManagementDecisions { get; set; } = null;

        /// <summary>
        /// A list of greening data associated with the farm for a particular year.
        /// </summary>
        [JsonIgnore]
        public List<GreeningFarmYearData>? GreeningFarmYearData { get; set; } = null;

        /// <summary>
        /// A list of land in-rent agreements where the farm is the origin.
        /// </summary>
        [JsonIgnore]
        [InverseProperty("OriginFarm")]
        public List<LandRent>? LandInRents { get; set; } = null;

        /// <summary>
        /// A list of land out-rent agreements where the farm is the destination.
        /// </summary>
        [JsonIgnore]
        [InverseProperty("DestinationFarm")]
        public List<LandRent>? LandOutRents { get; set; } = null;
    }

    /// <summary>
    /// Represents the altitude category of a farm.
    /// </summary>
    public enum AltitudeEnum
    {
        Mountains = 1,
        Hills = 2,
        Plains = 3
    }


}
