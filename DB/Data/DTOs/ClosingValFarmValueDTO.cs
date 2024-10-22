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
    /// Represents closing value farm data.
    /// </summary>
    public class ClosingValFarmValueDTO
    {
        /// <summary>
        /// Gets or sets the total area of agricultural land [ha].
        /// </summary>
        // Total Area of type Agricultural Land [ha]
        public float AgriculturalLandArea { get; set; }

        /// <summary>
        /// Gets or sets the total value of agricultural land [€].
        /// </summary>
        // Total value of Agricultural Land [€]
        public float AgriculturalLandValue { get; set; }

        /// <summary>
        /// Gets or sets the acquired agricultural land [ha].
        /// </summary>
        // Acquired Agricultural Land [ha]
        public float AgriculturalLandHectaresAdquisition { get; set; }

        /// <summary>
        /// Gets or sets the investment in land improvements [€].
        /// </summary>
        // Invesment in Land improvements [€]
        public float LandImprovements { get; set; }

        /// <summary>
        /// Gets or sets the total area of forest land [ha].
        /// </summary>
        // Total Area of type Forest Land [ha]
        public float ForestLandArea { get; set; }

        /// <summary>
        /// Gets or sets the total value of forest land [€].
        /// </summary>
        // Total value of Forest Land [€]
        public float ForestLandValue { get; set; }

        /// <summary>
        /// Gets or sets the value of buildings in the farm [€].
        /// </summary>
        // Value of Buildings in the farm [€]
        public float FarmBuildingsValue { get; set; }

        /// <summary>
        /// Gets or sets the value of machinery and equipment in the farm [€].
        /// </summary>
        // Value of Machinery and Equipment in the farm [€]
        public float MachineryAndEquipment { get; set; }

        /// <summary>
        /// Gets or sets the value of intangible assets that are tradable [€].
        /// </summary>
        // Value of intangible assets that are tradable [€]
        public float IntangibleAssetsTradable { get; set; }

        /// <summary>
        /// Gets or sets the value of intangible assets that are non-tradable [€].
        /// </summary>
        // Value of intangible assets that are non-tradable [€]
        public float IntangibleAssetsNonTradable { get; set; }

        /// <summary>
        /// Gets or sets the value of other non-current assets [€].
        /// </summary>
        // Value of other non-current assets [€]
        public float OtherNonCurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the total value of established long and medium term loans [€].
        /// </summary>
        // Total value of established long and medium term loans [€]
        public float LongAndMediumTermLoans { get; set; }

        /// <summary>
        /// Gets or sets the total value of current assets [€].
        /// </summary>
        // Total value of current assets [€]
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the farm net income [€].
        /// </summary>
        // Farm Net Income [€]
        public float FarmNetIncome { get; set; }

        /// <summary>
        /// Gets or sets the gross farm income [€].
        /// </summary>
        // Gross Farm Income [€]
        public float GrossFarmIncome { get; set; }

        /// <summary>
        /// Gets or sets the total value of subsidies on investments [€].
        /// </summary>
        // Total value of subsidies on investments [€]
        public float SubsidiesOnInvestments { get; set; }

        /// <summary>
        /// Gets or sets the balance of taxes on investments [€].
        /// </summary>
        // Balance of Taxes on Investments [€]
        public float VATBalanceOnInvestments { get; set; }

        /// <summary>
        /// Gets or sets the total value of agricultural production [€].
        /// </summary>
        // Total value of Agricultural Production [€]
        public float TotalOutputCropsAndCropProduction { get; set; }

        /// <summary>
        /// Gets or sets the total value of livestock production [€].
        /// </summary>
        // Total value of Livestock Production [€]
        public float TotalOutputLivestockAndLivestockProduction { get; set; }

        /// <summary>
        /// Gets or sets the total value of other outputs [€].
        /// </summary>
        // Total value of other outputs [€]
        public float OtherOutputs { get; set; }

        /// <summary>
        /// Gets or sets the total value of intermediate consumption [€].
        /// </summary>
        // Total value of intermediate consumption [€]
        public float TotalIntermediateConsumption { get; set; }

        /// <summary>
        /// Gets or sets the value of taxes (>0 received, <0 paid) [€].
        /// </summary>
        // Value of Taxes (>0 received , <0 paid) [€]
        public float Taxes { get; set; }

        /// <summary>
        /// Gets or sets the balance of VAT excluding investments [€].
        /// </summary>
        // Balance of VAT excluding investments [€]
        public float VatBalanceExcludingInvestments { get; set; }

        /// <summary>
        /// Gets or sets the total value of fixed assets [€].
        /// </summary>
        // Total value of Fixed Assets [€]
        public float FixedAssets { get; set; }

        /// <summary>
        /// Gets or sets the yearly depreciation [€].
        /// </summary>
        // Yearly Depreciation [€]
        public float Depreciation { get; set; }

        /// <summary>
        /// Gets or sets the total value of external factors [€].
        /// </summary>
        // Total value of External Factors [€]
        public float TotalExternalFactors { get; set; }

        /// <summary>
        /// Gets or sets the total value of machinery [€].
        /// </summary>
        // Total value of Machinery [€]
        public float Machinery { get; set; }

        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the balance (>0 received, <0 paid) of rent operations [€].
        /// </summary>
        // Balance (>0 received , <0 paid) of rent operations [€]
        public float RentBalance { get; set; }
    }
}

