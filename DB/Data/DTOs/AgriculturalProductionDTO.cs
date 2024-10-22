using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents agricultural production data.
    /// </summary>
    public class AgriculturalProductionJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Gets or sets the type of organic production.
        /// </summary>
        public string? OrganicProductionType { get; set; }

        /// <summary>
        /// Gets or sets the cultivated area (UAA - [ha]).
        /// </summary>
        // Utilized Agricultural Area (UAA - [ha])
        public float? CultivatedArea { get; set; }

        /// <summary>
        /// Gets or sets the irrigated area (IA - [ha]).
        /// </summary>
        // Irrigated Area (IA - [ha])
        public float? IrrigatedArea { get; set; }

        /// <summary>
        /// Gets or sets the value of total production (PLT - [€]).
        /// </summary>
        // Value of total production (PLT - [€])
        public float? CropProduction { get; set; }

        /// <summary>
        /// Gets or sets the quantity of sold production ([tons]).
        /// </summary>
        // Quantity of Sold Production ([tons])
        public float? QuantitySold { get; set; }

        /// <summary>
        /// Gets or sets the quantity of used production ([tons]).
        /// </summary>
        // Quantity of Used Production ([tons])
        public float? QuantityUsed { get; set; }

        /// <summary>
        /// Gets or sets the value of sales (PLV - [€]).
        /// </summary>
        // Value of Sales (PLV - [€])
        public float? ValueSales { get; set; }

        /// <summary>
        /// Gets or sets the variable costs per produced unit (CV - [€/ton]).
        /// </summary>
        // Variable Costs per produced unit (CV - [€/ton])
        public float? VariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the land value (PVF - [€]).
        /// </summary>
        // Land Value (PVF - [€])
        public float? LandValue { get; set; }

        /// <summary>
        /// Gets or sets the unit selling price (PVU - [€/unit]).
        /// </summary>
        // Unit selling price (PVU - [€/unit])
        public float? SellingPrice { get; set; }
    }

    /// <summary>
    /// Represents agricultural production data transfer object.
    /// </summary>
    public class AgriCulturalProductionDTO
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// Gets or sets the year ID.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the product group ID.
        /// </summary>
        public long? ProductGroupId { get; set; }

        /// <summary>
        /// Gets or sets the farm ID.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// Gets or sets the type of organic production.
        /// </summary>
        public int? OrganicProductionType { get; set; }

        /// <summary>
        /// Gets or sets the cultivated area (UAA - [ha]).
        /// </summary>
        // Utilized Agricultural Area (UAA - [ha])
        public float? CultivatedArea { get; set; }

        /// <summary>
        /// Gets or sets the irrigated area (IA - [ha]).
        /// </summary>
        // Irrigated Area (IA - [ha])
        public float? IrrigatedArea { get; set; }

        /// <summary>
        /// Gets or sets the value of total production (PLT - [€]).
        /// </summary>
        // Value of total production (PLT - [€])
        public float? CropProduction { get; set; }

        /// <summary>
        /// Gets or sets the quantity of sold production ([tons]).
        /// </summary>
        // Quantity of Sold Production ([tons])
        public float? QuantitySold { get; set; }

        /// <summary>
        /// Gets or sets the quantity of used production ([tons]).
        /// </summary>
        // Quantity of Used Production ([tons])
        public float? QuantityUsed { get; set; }

        /// <summary>
        /// Gets or sets the value of sales (PLV - [€]).
        /// </summary>
        // Value of Sales (PLV - [€])
        public float? ValueSales { get; set; }

        /// <summary>
        /// Gets or sets the variable costs per produced unit (CV - [€/ton]).
        /// </summary>
        // Variable Costs per produced unit (CV - [€/ton])
        public float? VariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the land value (PVF - [€]).
        /// </summary>
        // Land Value (PVF - [€])
        public float? LandValue { get; set; }

        /// <summary>
        /// Gets or sets the unit selling price (PVU - [€/unit]).
        /// </summary>
        // Unit selling price (PVU - [€/unit])
        public float? SellingPrice { get; set; }
    }
}

