using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a specific year within a population, including data related to that year.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    [SwaggerSchema(Required = new[] { "Id" })]
    [Index(nameof(YearNumber), nameof(PopulationId), IsUnique = true)]
    [Index(nameof(Id), nameof(PopulationId), IsUnique = true)]
    public class Year : Entity
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the population associated with this year.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the population associated with this year.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of farm year subsidies associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<FarmYearSubsidy>? FarmYearSubsidies { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of holder farm year data associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<HolderFarmYearData>? HoldersFarmYearData { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of simulation scenarios associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<SimulationScenario>? SimulationScenarios { get; set; }

        /// <summary>
        /// Gets or sets the list of synthetic populations associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<SyntheticPopulation>? SyntheticPopulations { get; set; }

        /// <summary>
        /// Gets or sets the list of agricultural productions associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<AgriculturalProduction>? AgriculturalProductions { get; set; }

        /// <summary>
        /// Gets or sets the list of livestock productions associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<LivestockProduction>? LivestockProductions { get; set; }

        /// <summary>
        /// Gets or sets the list of land transactions associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<LandTransaction>? LandTransactions { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of agro-management decisions associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<AgroManagementDecision>? AgroManagementDecisions { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of greening farm year data associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<GreeningFarmYearData>? GreeningFarmYearData { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of land rents associated with this year.
        /// </summary>
        [JsonIgnore]
        public List<LandRent>? LandRents { get; set; } = null;
    }
}
