using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a data transfer object (DTO) for values to be used in linear programming (LP),
    /// including farm details, assets, and financial information.
    /// </summary>
    [NotMapped]
    public class ValueToLPDTO
    {
        /// <summary>
        /// Gets or sets the farm identifier.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// Gets or sets the year identifier.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the total area of agricultural land in hectares.
        /// </summary>
        public float AgriculturalLandArea { get; set; }

        /// <summary>
        /// Gets or sets the total value of the owned agricultural land in euros.
        /// </summary>
        public float AgriculturalLandValue { get; set; }

        /// <summary>
        /// Gets or sets the current assets in the SP model in euros.
        /// </summary>
        public float SE465 { get; set; }

        /// <summary>
        /// Gets or sets the SE490 value in euros.
        /// </summary>
        public float SE490 { get; set; }

        /// <summary>
        /// Gets or sets the average price per hectare in euros per hectare.
        /// </summary>
        public float? AverageHAPrice { get; set; }

        /// <summary>
        /// Gets or sets the SE420 value in euros.
        /// </summary>
        public float SE420 { get; set; }

        /// <summary>
        /// Gets or sets the SE410 value in euros.
        /// </summary>
        public float SE410 { get; set; }

        /// <summary>
        /// Gets or sets the aversion risk factor.
        /// </summary>
        public float AversionRiskFactor { get; set; }

        /// <summary>
        /// Gets or sets the holder farm year data associated with the agent.
        /// </summary>
        public HolderFarmYearDataJsonDTO AgentHolder { get; set; }

        /// <summary>
        /// Gets or sets the list of farm year subsidies associated with the agent.
        /// </summary>
        public List<FarmYearSubsidyDTO> AgentSubsidies { get; set; }

        /// <summary>
        /// Gets or sets the region level 3 identifier.
        /// </summary>
        public long RegionLevel3 { get; set; }
    }
    /// <summary>
    /// Represents a data transfer object (DTO) for data used in linear programming (LP),
    /// including values, agricultural productions, policies, and rent operations.
    /// </summary>
    [NotMapped]
    public class DataToLPDTO
    {
        /// <summary>
        /// Gets or sets the list of values used in the linear programming (LP).
        /// </summary>
        public List<ValueToLPDTO> Values { get; set; }

        /// <summary>
        /// Gets or sets the list of agricultural productions.
        /// </summary>
        public List<AgriCulturalProductionDTO>? AgriculturalProductions { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of policy group relations.
        /// </summary>
        public List<PolicyGroupRelationJsonDTO>? PolicyGroupRelations { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of policies.
        /// </summary>
        public List<PolicyJsonDTO>? Policies { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore linear programming (LP).
        /// </summary>
        public bool IgnoreLP { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the land market model (LMM).
        /// </summary>
        public bool IgnoreLMM { get; set; }

        /// <summary>
        /// Gets or sets the list of land rent operations.
        /// </summary>
        public List<LandRentDTO>? RentOperations { get; set; } = null;
    }

    /// <summary>
    /// Represents a data transfer object (DTO) for agro-management decisions,
    /// including farm details, land acquisitions, and financial decisions.
    /// </summary>
    [NotMapped]
    public class AgroManagementDecisionDTO
    {
        /// <summary>
        /// Gets or sets the farm identifier.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// Gets or sets the year identifier.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the total agricultural land of the farm after land transfers in the year market, in hectares.
        /// </summary>
        public float AgriculturalLand { get; set; }

        /// <summary>
        /// Gets or sets the total current assets of the farm after acquiring planned loans and land transfers, in euros.
        /// </summary>
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the value of long and medium-term loans after the establishment of new planned loans, in euros.
        /// </summary>
        public float LongAndMediumTermLoans { get; set; }

        /// <summary>
        /// Gets or sets the average value per hectare of the farm land, in euros per hectare.
        /// </summary>
        public float AverageLandValue { get; set; }

        /// <summary>
        /// Gets or sets the total area of land the farmer was willing to acquire, in hectares.
        /// </summary>
        public float TargetedLandAquisitionArea { get; set; }

        /// <summary>
        /// Gets or sets the price per hectare the farmer was willing to pay for the land, in euros per hectare.
        /// </summary>
        public float TargetedLandAquisitionHectarPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the farmer is willing to retire and hand over the farm to successors.
        /// </summary>
        public bool RetireAndHandOver { get; set; }
    }
    /// <summary>
    /// Represents a data transfer object (DTO) for agro-management decisions derived from linear programming (LP),
    /// including decisions, land transactions, and a list of errors.
    /// </summary>
    [NotMapped]
    public class AgroManagementDecisionFromLP
    {
        /// <summary>
        /// Gets or sets the list of agro-management decisions.
        /// </summary>
        public List<AgroManagementDecisionDTO> AgroManagementDecisions { get; set; }

        /// <summary>
        /// Gets or sets the list of land transactions.
        /// </summary>
        public List<LandTransactionDTO> LandTransactions { get; set; }

        /// <summary>
        /// Gets or sets the list of errors.
        /// </summary>
        public List<long> errorList { get; set; }
    }
}