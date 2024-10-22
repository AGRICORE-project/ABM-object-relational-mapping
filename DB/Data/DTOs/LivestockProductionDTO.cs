using DB.Data.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a JSON data transfer object (DTO) for livestock production data, 
    /// including various metrics such as the number of animals, milk production, and associated financial values.
    /// </summary>
    public class LivestockProductionJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number associated with the livestock production data.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the product associated with the livestock production.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Gets or sets the number of animals in units.
        /// </summary>
        // Number of Animals [units]
        public float NumberOfAnimals { get; set; }

        /// <summary>
        /// Gets or sets the number of dairy cows in UBA (units).
        /// </summary>
        // Number of dairy cows [UBA - [units]]
        public int DairyCows { get; set; }

        /// <summary>
        /// Gets or sets the number of animals sold in units.
        /// </summary>
        // Number of Animals Sold [units]
        public int NumberOfAnimalsSold { get; set; }

        /// <summary>
        /// Gets or sets the value of sold animals in Euros.
        /// </summary>
        // Value of Sold Animals ([€])
        public float ValueSoldAnimals { get; set; }

        /// <summary>
        /// Gets or sets the number of animals sent for slaughtering in units.
        /// </summary>
        // Number of Animals for Slaughtering [units]
        public int NumberAnimalsForSlaughtering { get; set; }

        /// <summary>
        /// Gets or sets the value of slaughtered animals in Euros.
        /// </summary>
        // Value of Slaughtered Animals ([€])
        public float ValueSlaughteredAnimals { get; set; }

        /// <summary>
        /// Gets or sets the number of animals used for rearing or breeding in units.
        /// </summary>
        // Number of Animals for Rearing/Breeding [units]
        public float NumberAnimalsRearingBreading { get; set; }

        /// <summary>
        /// Gets or sets the value of animals used for rearing or breeding in Euros.
        /// </summary>
        // Value of Animals for Rearing/Breeding ([€])
        public float ValueAnimalsRearingBreading { get; set; }

        /// <summary>
        /// Gets or sets the total milk production in tons.
        /// </summary>
        // Number of tons of milk produced [tons]
        public float MilkTotalProduction { get; set; }

        /// <summary>
        /// Gets or sets the amount of milk production sold in tons.
        /// </summary>
        // Number of tons of milk sold [tons]
        public float MilkProductionSold { get; set; }

        /// <summary>
        /// Gets or sets the total sales value of milk in Euros.
        /// </summary>
        // Value of milk sold ([€])
        public float MilkTotalSales { get; set; }

        /// <summary>
        /// Gets or sets the variable costs per produced unit of milk in Euros per ton.
        /// </summary>
        // Variable Costs per produced unit (CV - [€/ton])
        public float MilkVariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the total wool production in tons.
        /// </summary>
        public float WoolTotalProduction { get; set; }

        /// <summary>
        /// Gets or sets the amount of wool production sold in tons.
        /// </summary>
        public float WoolProductionSold { get; set; }

        /// <summary>
        /// Gets or sets the total sales value of eggs in Euros.
        /// </summary>
        public float EggsTotalSales { get; set; }

        /// <summary>
        /// Gets or sets the total egg production in tons.
        /// </summary>
        public float EggsTotalProduction { get; set; }

        /// <summary>
        /// Gets or sets the amount of egg production sold in tons.
        /// </summary>
        public float EggsProductionSold { get; set; }

        /// <summary>
        /// Gets or sets the total sales value of manure in Euros.
        /// </summary>
        public float ManureTotalSales { get; set; }

        /// <summary>
        /// Gets or sets the average variable cost per unit of product in Euros per ton.
        /// </summary>
        // Average variable cost per unit of product[€/ ton]
        public float VariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the average selling price per unit of product in Euros per ton.
        /// </summary>
        // Average sell price per unit of product[€/ ton]
        public float? SellingPrice { get; set; }
    }

}