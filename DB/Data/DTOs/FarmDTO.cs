using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents farm data.
    /// </summary>
    public class FarmJsonDTO
    {
        /// <summary>
        /// Gets or sets the farm code.
        /// </summary>
        public string FarmCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public long Lat { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public long Long { get; set; }

        /// <summary>
        /// Gets or sets the altitude.
        /// </summary>
        public string Altitude { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the level 1 region.
        /// </summary>
        public string RegionLevel1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the level 1 region.
        /// </summary>
        public string RegionLevel1Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the level 2 region.
        /// </summary>
        public string RegionLevel2 { get; set; }

        /// <summary>
        /// Gets or sets the name of the level 2 region.
        /// </summary>
        public string RegionLevel2Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the level 3 region.
        /// </summary>
        public long RegionLevel3 { get; set; }

        /// <summary>
        /// Gets or sets the name of the level 3 region.
        /// </summary>
        public string RegionLevel3Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the technical economic orientation.
        /// </summary>
        public int TechnicalEconomicOrientation { get; set; }

        /// <summary>
        /// Gets or sets the agricultural productions.
        /// </summary>
        
        public List<AgriculturalProductionJsonDTO> AgriculturalProductions { get; set; } = new List<AgriculturalProductionJsonDTO>();

        /// <summary>
        /// Gets or sets the livestock productions.
        /// </summary>
        public List<LivestockProductionJsonDTO> LivestockProductions { get; set; } = new List<LivestockProductionJsonDTO>();

        /// <summary>
        /// Gets or sets the holder farm year data.
        /// </summary>
        public List<HolderFarmYearDataJsonDTO> HolderFarmYearsData { get; set; } = new List<HolderFarmYearDataJsonDTO>();

        /// <summary>
        /// Gets or sets the closing value farm data.
        /// </summary>
        public List<ClosingValFarmValueDTO> ClosingValFarmValues { get; set; } = new List<ClosingValFarmValueDTO>();

        /// <summary>
        /// Gets or sets the farm year subsidies.
        /// </summary>
        public List<FarmYearSubsidyDTO> FarmYearSubsidies { get; set; } = new List<FarmYearSubsidyDTO>();

        /// <summary>
        /// Gets or sets the agro management decisions.
        /// </summary>
        public List<AgroManagementDecisionJsonDTO> AgroManagementDecisions { get;set; } = new List<AgroManagementDecisionJsonDTO>();

        /// <summary>
        /// Gets or sets the greening farm year data.
        /// </summary>
        public List<GreeningFarmYearDataJsonDTO> GreeningFarmYearData { get; set; } = new List<GreeningFarmYearDataJsonDTO>();

        // Land Transactions cannot be included in the DTO because they may relate to other farms not yet included in the DB
        // public List<LandTransactionJsonDTO> LandTransactions { get; set; } = new List<LandTransactionJsonDTO>();
        
    }
}
