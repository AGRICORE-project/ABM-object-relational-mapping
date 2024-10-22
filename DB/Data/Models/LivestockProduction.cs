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
    [Index(nameof(FarmId), nameof(ProductGroupId), nameof(YearId), IsUnique = true)]
    /// <summary>
    /// Represents livestock production data for a specific farm, product group, and year.
    /// </summary>
    public class LivestockProduction : Entity
    {
        /// <summary>
        /// The product group ID associated with this livestock production.
        /// </summary>
        public long ProductGroupId { get; set; }

        /// <summary>
        /// The product group associated with this livestock production.
        /// </summary>
        [JsonIgnore]
        public ProductGroup? ProductGroup { get; set; } = null!;

        /// <summary>
        /// The farm ID associated with this livestock production.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// The farm associated with this livestock production.
        /// </summary>
        [JsonIgnore]
        public Farm? Farm { get; set; } = null!;

        /// <summary>
        /// The year ID associated with this livestock production.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// The year associated with this livestock production.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; } = null!;

        /// <summary>
        /// The total number of animals.
        /// </summary>
        //data type changed to float due to origin data being fractional value
        public float NumberOfAnimals { get; set; }

        /// <summary>
        /// The number of dairy cows.
        /// </summary>
        public int DairyCows { get; set; }

        /// <summary>
        /// The number of animals sold.
        /// </summary>
        public int NumberOfAnimalsSold { get; set; }

        /// <summary>
        /// The value of sold animals in euros.
        /// </summary>
        public float ValueSoldAnimals { get; set; }

        /// <summary>
        /// The number of animals for slaughtering.
        /// </summary>
        public int NumberAnimalsForSlaughtering { get; set; }

        /// <summary>
        /// The value of slaughtered animals in euros.
        /// </summary>
        public float ValueSlaughteredAnimals { get; set; }

        /// <summary>
        /// The number of animals for rearing and breeding.
        /// </summary>
        //data type changed to float due to origin data being fractional value
        public float NumberAnimalsRearingBreading { get; set; }

        /// <summary>
        /// The value of animals for rearing and breeding in euros.
        /// </summary>
        public float ValueAnimalsRearingBreading { get; set; }

        /// <summary>
        /// The total production of milk in tons.
        /// </summary>
        // Number of tons of milk produced [tons]
        public float MilkTotalProduction { get; set; }

        /// <summary>
        /// The amount of milk sold in tons.
        /// </summary>
        // Number of tons of milk sold [tons]
        public float MilkProductionSold { get; set; }

        /// <summary>
        /// The total sales value of milk in euros.
        /// </summary>
        // value of milk sold [€] 
        public float MilkTotalSales { get; set; }

        /// <summary>
        /// The variable costs of milk production per unit in euros per ton.
        /// </summary>
        // Variable costs of milk production per unit [€/ton]
        public float MilkVariableCosts { get; set; }

        /// <summary>
        /// The total production of wool in tons.
        /// </summary>
        // Number of tons of wool [tons] 
        public float WoolTotalProduction { get; set; }

        /// <summary>
        /// The amount of wool sold in tons.
        /// </summary>
        // Number of tons of wool sold [tons] 
        public float WoolProductionSold { get; set; }

        /// <summary>
        /// The total sales value of eggs in euros.
        /// </summary>
        // Value of sold eggs [€]
        public float EggsTotalSales { get; set; }

        /// <summary>
        /// The total production of eggs in tons.
        /// </summary>
        // Number of tons of eggs produced [tons]
        public float EggsTotalProduction { get; set; }

        /// <summary>
        /// The amount of eggs sold in tons.
        /// </summary>
        // Number of tons of eggs sold [tons]
        public float EggsProductionSold { get; set; }

        /// <summary>
        /// The total sales value of manure in euros.
        /// </summary>
        // Value of sold manure [€] 
        public float ManureTotalSales { get; set; }

        /// <summary>
        /// The variable costs of the livestock production.
        /// </summary>
        public float VariableCosts { get; set; }

        /// <summary>
        /// The selling price, if available.
        /// </summary>
        public float? SellingPrice { get; set; }
    }
}
