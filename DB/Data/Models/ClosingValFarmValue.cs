using DB.Data.DTOs;
using DB.Data.Repositories;
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
    /// Represents the closing valuation of farm assets and income for a specific farm and year.
    /// </summary>
    [Index(nameof(FarmId), nameof(YearId), IsUnique = true)]
    public class ClosingValFarmValue : Entity
    {
        /// <summary>
        /// The value of agricultural land, specified as "Greek: A2" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A2 but need to be dissagregated")]
        public float AgriculturalLandValue { get; set; }

        /// <summary>
        /// The area of agricultural land in hectares.
        /// </summary>
        public float AgriculturalLandArea { get; set; }

        /// <summary>
        /// The value of land improvements, specified as "Greek: A2" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A2 but need to be dissagregated")]
        public float LandImprovements { get; set; }

        /// <summary>
        /// The value of plantations.
        /// </summary>
        public float PlantationsValue { get; set; }

        /// <summary>
        /// The value of forest land, specified as "Greek: A2" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A2 but need to be dissagregated")]
        public float ForestLandValue { get; set; }

        /// <summary>
        /// The area of forest land in hectares.
        /// </summary>
        public float ForestLandArea { get; set; }

        /// <summary>
        /// The value of farm buildings, specified as "Greek: A3" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A3 but need to be dissagregated")]
        public float FarmBuildingsValue { get; set; }

        /// <summary>
        /// The value of machinery and equipment, specified as "Greek: A3" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A3 but need to be dissagregated")]
        public float MachineryAndEquipment { get; set; }

        /// <summary>
        /// The value of intangible tradable assets, specified as "Greek: A4" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A4 but need to be dissagregated")]
        public float IntangibleAssetsTradable { get; set; }

        /// <summary>
        /// The value of intangible non-tradable assets, specified as "Greek: A4" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A4 but need to be dissagregated")]
        public float IntangibleAssetsNonTradable { get; set; }

        /// <summary>
        /// The value of other non-current assets, specified as "Greek: A4" but needs to be disaggregated.
        /// </summary>
        [Display(Description = "Greek: A4 but need to be dissagregated")]
        public float OtherNonCurrentAssets { get; set; }

        /// <summary>
        /// The amount of long and medium term loans, specified as "SE490".
        /// </summary>
        [Display(Description = "SE490")]
        public float LongAndMediumTermLoans { get; set; }

        /// <summary>
        /// Total current assets including FADN 1010, 1020, 1030, 1040, and 2010 values, specified as "SE465".
        /// </summary>
        [Display(Description = "Include FADN 1010, 1020, 1030, 1040 and 2010 values = SE465")]
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// The net income of the farm, specified as "SE420".
        /// </summary>
        [Display(Description = "SE420")]
        public float FarmNetIncome { get; set; }

        /// <summary>
        /// The gross income of the farm, specified as "SE410".
        /// </summary>
        [Display(Description = "SE410")]
        public float GrossFarmIncome { get; set; }

        /// <summary>
        /// The subsidies received on investments, specified as "SE406".
        /// </summary>
        [Display(Description = "SE406")]
        public float SubsidiesOnInvestments { get; set; }

        /// <summary>
        /// The VAT balance on investments, specified as "SE408".
        /// </summary>
        [Display(Description = "SE408")]
        public float VATBalanceOnInvestments { get; set; }

        /// <summary>
        /// The total output of crops and crop production, specified as "SE135".
        /// </summary>
        [Display(Description = "SE135")]
        public float TotalOutputCropsAndCropProduction { get; set; }

        /// <summary>
        /// The total output of livestock and livestock production, specified as "SE206".
        /// </summary>
        [Display(Description = "SE206")]
        public float TotalOutputLivestockAndLivestockProduction { get; set; }

        /// <summary>
        /// Other outputs, specified as "SE256".
        /// </summary>
        [Display(Description = "SE256")]
        public float OtherOutputs { get; set; }

        /// <summary>
        /// Total intermediate consumption, specified as "SE275".
        /// </summary>
        [Display(Description = "SE275")]
        public float TotalIntermediateConsumption { get; set; }

        /// <summary>
        /// The amount of taxes, specified as "SE390".
        /// </summary>
        [Display(Description = "SE390")]
        public float Taxes { get; set; }

        /// <summary>
        /// The VAT balance excluding investments, specified as "SE395".
        /// </summary>
        [Display(Description = "SE395")]
        public float VatBalanceExcludingInvestments { get; set; }

        /// <summary>
        /// The value of fixed assets, specified as "SE441".
        /// </summary>
        [Display(Description = "SE441")]
        public float FixedAssets { get; set; }

        /// <summary>
        /// The depreciation value, specified as "SE360".
        /// </summary>
        [Display(Description = "SE360 - Requires A2-A4 but dissagregated and split into OV AD and DY")]
        public float Depreciation { get; set; }

        /// <summary>
        /// Total external factors, specified as "SE365".
        /// </summary>
        [Display(Description = "SE365")]
        public float TotalExternalFactors { get; set; }

        /// <summary>
        /// The value of machinery, specified as "SE455".
        /// </summary>
        [Display(Description = "SE455 - From A3 but need to be dissagregated")]
        public float Machinery { get; set; }

        /// <summary>
        /// The farm Id associated with this closing valuation.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// The farm associated with this closing valuation.
        /// </summary>
        [JsonIgnore]
        public Farm? Farm { get; set; } = null!;

        /// <summary>
        /// The year Id associated with this closing valuation.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// The year associated with this closing valuation.
        /// </summary>
        [JsonIgnore]
        public Year? Year { get; set; } = null!;

        /// <summary>
        /// The rent balance, where positive value is for incomes and negative value for costs.
        /// </summary>
        [Display(Description = "positive value is for incomes and negative value for costs")]
        public float RentBalance { get; set; }
    }
}
