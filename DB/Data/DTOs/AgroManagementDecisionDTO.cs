using DB.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents agricultural management decision data.
    /// </summary>
    public class AgroManagementDecisionJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the total area of agricultural land [ha].
        /// </summary>
        // Total Area of type Agricultural Land[ha]
        public float AgriculturalLandArea { get; set; }

        /// <summary>
        /// Gets or sets the total value of owned agricultural land [€].
        /// </summary>
        // Total value of the owned Agricultural Land [€]
        public float AgriculturalLandValue { get; set; }

        /// <summary>
        /// Gets or sets the amount of established loans at long and medium term [€].
        /// </summary>
        // Amount of stablished loans at long and medium term [€]
        public float LongAndMediumTermLoans { get; set; }

        /// <summary>
        /// Gets or sets the total current assets [€].
        /// </summary>
        // Total current assets [€]
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the average hectare price of the owned land [€/ha].
        /// </summary>
        // Average hectar price of the owned land [€/ha]
        public float AverageLandValue { get; set; }

        /// <summary>
        /// Gets or sets the total amount of land the farmer is willing to acquire [ha].
        /// </summary>
        // Total amount of land the farmer is willing to acquire [ha]
        public float TargetedLandAquisitionArea { get; set; }

        /// <summary>
        /// Gets or sets the price per hectare the farmer is willing to pay for the land [€/ha].
        /// </summary>
        // Price per hectar the farmer is willing to pay for the land [€/ha]
        public float TargetedLandAquisitionHectarPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the farmer is willing to retire and hand over the farm to its successors.
        /// </summary>
        // Boolean to indicate if the farmer is willing to retire and hand over the farm to its successors
        public bool RetireAndHandOver { get; set; }
    }
}
