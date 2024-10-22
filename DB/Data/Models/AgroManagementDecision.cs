using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a management decision for a specific farm and year.
    /// </summary>
    [Index(nameof(FarmId), nameof(YearId), IsUnique = true)]
    public class AgroManagementDecision : Entity
    {
        /// <summary>
        /// The farm Id associated with this management decision.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// The year Id associated with this management decision.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Total area of agricultural land in hectares.
        /// </summary>
        // Total Area of type Agricultural Land[ha]
        public float AgriculturalLandArea { get; set; }

        /// <summary>
        /// Total value of the owned agricultural land in euros.
        /// </summary>
        // Total value of the owned Agricultural Land [€]
        public float AgriculturalLandValue { get; set; }

        /// <summary>
        /// Amount of established loans at long and medium term in euros.
        /// </summary>
        // Amount of stablished loans at long and medium term [€]
        public float LongAndMediumTermLoans { get; set; }

        /// <summary>
        /// Total current assets in euros.
        /// </summary>
        // Total current assets [€]
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// Average hectare price of the owned land in euros per hectare.
        /// </summary>
        // Average hectar price of the owned land [€/ha]
        public float AverageLandValue { get; set; }

        /// <summary>
        /// Total amount of land the farmer is willing to acquire in hectares.
        /// </summary>
        // Total amount of land the farmer was willing to acquire [ha]
        public float TargetedLandAquisitionArea { get; set; }

        /// <summary>
        /// Price per hectare the farmer is willing to pay for the land in euros per hectare.
        /// </summary>
        // Price per hectar the farmer is willing to pay for the land [€/ha]
        public float TargetedLandAquisitionHectarPrice { get; set; }

        /// <summary>
        /// Indicates if the farmer is willing to retire and hand over the farm to its successors.
        /// </summary>
        // Boolean to indicate if the farmer is willing to retire and hand over the farm to its successors
        public bool RetireAndHandOver { get; set; }

        #region relations
        /// <summary>
        /// The farm associated with this management decision.
        /// </summary>
        [JsonIgnore]
        public Farm? Farm { get; set; } = null!;

        /// <summary>
        /// The year associated with this management decision.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; } = null!;
        #endregion
    }

}
