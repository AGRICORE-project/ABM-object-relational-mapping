using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents the agricultural production details for a specific farm, year, and product group.
    /// </summary>
    [Index(nameof(FarmId), nameof(ProductGroupId), nameof(YearId), IsUnique = true)]
    public class AgriculturalProduction : Entity
    {
        /// <summary>
        /// The product group Id to which the agricultural production belongs to.
        /// </summary>
        [Description("The product group Id to which the agricultural production belongs to.")]
        public long? ProductGroupId { get; set; }

        /// <summary>
        /// The product group to which the agricultural production belongs to.
        /// </summary>
        [Description("The product group to which the agricultural production belongs to.")]
        [JsonIgnore]
        public ProductGroup? ProductGroup { get; set; } = null!;

        /// <summary>
        /// The year (Id) to which the agricultural production belongs to.
        /// </summary>
        [Description("The year (Id) to which the agricultural production belongs to.")]
        public long YearId { get; set; }

        /// <summary>
        /// The year to which the agricultural production belongs to.
        /// </summary>
        [Description("The year to which the agricultural production belongs to.")]
        [JsonIgnore]
        public Year? Year { get; set; } = null!;

        /// <summary>
        /// The farm Id to which the agricultural production belongs to.
        /// </summary>
        [Description("The farm Id to which the agricultural production belongs to.")]
        public long FarmId { get; set; }

        /// <summary>
        /// The farm to which the agricultural production belongs to.
        /// </summary>
        [Description("The farm to which the agricultural production belongs to.")]
        [JsonIgnore]
        public Farm? Farm { get; set; } = null!;

        /// <summary>
        /// The type of organic production. See OrganicProductionType description.
        /// </summary>
        [Description("The typo of organic production. See OrganicProductionType description")]
        public OrganicProductionType OrganicProductionType { get; set; }

        /// <summary>
        /// The total cultivated area of the crop, in hectares, including owned and rented land.
        /// </summary>
        [Description("The total cultivated are of the crop, in hectares, including owned and rented land.")]
        public float? CultivatedArea { get; set; }

        /// <summary>
        /// The total irrigated area of the crop, in hectares, including owned and rented land.
        /// </summary>
        [Description("The total irrigated are of the crop, in hectares, including owned and rented land.")]
        public float? IrrigatedArea { get; set; }

        /// <summary>
        /// Value of total production (PLT - [€]).
        /// </summary>
        [Description(" Value of total production (PLT - [€])")]
        public float? CropProduction { get; set; }

        /// <summary>
        /// The total quantity of the crop sold, in tonnes.
        /// </summary>
        [Description("The total quantity of the crop sold, in tonnes.")]
        public float? QuantitySold { get; set; }

        /// <summary>
        /// The total quantity of the crop used for own consumption, in tonnes.
        /// </summary>
        [Description("The total quantity of the crop used for own consumption, in tonnes.")]
        public float? QuantityUsed { get; set; }

        /// <summary>
        /// The total value of the crop sold, in euros.
        /// </summary>
        [Description("The total value of the crop sold, in euros.")]
        public float? ValueSales { get; set; }

        /// <summary>
        /// The land transaction (if any) associated to this production.
        /// </summary>
        [Description("The land transaction (if any) associated to this production.")]
        [JsonIgnore]
        public LandTransaction? LandTransaction { get; set; } = null!;

        /// <summary>
        /// The variable costs associated with the production of one unit of the crop, in euros.
        /// </summary>
        [Description("The variable costs associated to the production of one unit of the crop, in euros.")]
        public float? VariableCosts { get; set; }

        /// <summary>
        /// The value of the land owned for this crop, not including rented land, in euros.
        /// </summary>
        [Description("The value of the land owned of this crop, not including rented land, in euros.")]
        public float? LandValue { get; set; }

        /// <summary>
        /// The single unit selling price of the crop, in euros.
        /// </summary>
        [Description("The single unit selling price of the crop, in euros.")]
        public float? SellingPrice { get; set; }
    }

    /// <summary>
    /// The type of organic production. 0= Conventional, 1 = Organic, 2= Undetermined.
    /// </summary>
    [Description("The typo of organic production. 0= Conventional, 1 = Organic, 2= Undetermined")]
    public enum OrganicProductionType
    {
        Conventional = 0,
        Organic = 1,
        Undetermined = 2
    }

}
