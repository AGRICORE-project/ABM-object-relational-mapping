using DB.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.DTOs
{
    [NotMapped]
    /// <summary>
    /// Represents a data transfer object (DTO) for values derived from the SP model,
    /// including farm assets, income, land, crops, and subsidies.
    /// </summary>
    public class ValueFromSPDTO
    {
        /// <summary>
        /// Gets or sets the farm identifier.
        /// </summary>
        public long FarmId { get; set; }

        /// <summary>
        /// Gets or sets the total current assets in euros.
        /// </summary>
        public float TotalCurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the farm net income in euros.
        /// </summary>
        public float FarmNetIncome { get; set; }

        /// <summary>
        /// Gets or sets the gross farm income in euros.
        /// </summary>
        public float FarmGrossIncome { get; set; }

        /// <summary>
        /// Gets or sets the area of agricultural land in hectares.
        /// </summary>
        public float AgriculturalLand { get; set; }

        /// <summary>
        /// Gets or sets the crops associated with the farm.
        /// </summary>
        public Dictionary<string, CropDataDTO>? Crops { get; set; }

        /// <summary>
        /// Gets or sets the list of subsidies associated with the farm year.
        /// </summary>
        public List<FarmYearSubsidyDTO> Subsidies { get; set; }

        /// <summary>
        /// Gets or sets the total variable costs in euros.
        /// </summary>
        public float TotalVariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the balance of the rented area, in hectares. A positive value indicates renting in land.
        /// </summary>
        public float RentBalanceArea { get; set; }

        /// <summary>
        /// Gets or sets the greening surface in hectares.
        /// </summary>
        public float GreeningSurface { get; set; }

        /// <summary>
        /// Gets or sets the list of land rents where the destination farm is this one.
        /// </summary>
        public List<LandRentDTO>? RentedInLands { get; set; } = new List<LandRentDTO>();
    }

    /// <summary>
    /// Represents a data transfer object (DTO) for data sent to the SP model,
    /// including values, product groups, policies, and subsidies.
    /// </summary>
    [NotMapped]
    public class DataToSPDTO
    {
        /// <summary>
        /// Gets or sets the list of values sent to the SP model.
        /// </summary>
        public List<ValueToSPDTO>? Values { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of product groups.
        /// </summary>
        public List<ProductGroupJsonDTO>? ProductGroups { get; set; } = null;

        /// <summary>
        /// Gets or sets the list of policies.
        /// </summary>
        public List<PolicyJsonDTO> Policies { get; set; } = new List<PolicyJsonDTO>();

        /// <summary>
        /// Gets or sets the list of policy group relations.
        /// </summary>
        public List<PolicyGroupRelationJsonDTO> PolicyGroupRelations { get; set; } = new List<PolicyGroupRelationJsonDTO>();

        /// <summary>
        /// Gets or sets the list of farm year subsidies.
        /// </summary>
        public List<FarmYearSubsidyDTO> FarmYearSubsidies { get; set; } = new List<FarmYearSubsidyDTO>();
    }

    /// <summary>
    /// Represents a data transfer object (DTO) for values sent to the SP model,
    /// including farm code, holder information, agricultural land, crops, and livestock.
    /// </summary>
    [NotMapped]
    public class ValueToSPDTO
    {
        /// <summary>
        /// Gets or sets the farm code.
        /// </summary>
        public long FarmCode { get; set; }

        /// <summary>
        /// Gets or sets the holder information.
        /// </summary>
        public HolderInfoDTO? HolderInfo { get; set; } = null;

        /// <summary>
        /// Gets or sets the COD_RAGR value.
        /// </summary>
        public string Cod_RAGR { get; set; }

        /// <summary>
        /// Gets or sets the COD_RAGR2 value.
        /// </summary>
        public string Cod_RAGR2 { get; set; }

        /// <summary>
        /// Gets or sets the COD_RAGR3 value.
        /// </summary>
        public long Cod_RAGR3 { get; set; }

        /// <summary>
        /// Gets or sets the technical economic orientation.
        /// </summary>
        public int TechnicalEconomicOrientation { get; set; }

        /// <summary>
        /// Gets or sets the altitude.
        /// </summary>
        public AltitudeEnum Altitude { get; set; }

        /// <summary>
        /// Gets or sets the current assets in euros.
        /// </summary>
        public float CurrentAssets { get; set; }

        /// <summary>
        /// Gets or sets the crops associated with the farm.
        /// </summary>
        public Dictionary<string, CropDataDTO>? Crops { get; set; }

        /// <summary>
        /// Gets or sets the livestock associated with the farm.
        /// </summary>
        public LivestockDTO? Livestock { get; set; } = null;

        /// <summary>
        /// Gets or sets the greening surface in hectares.
        /// </summary>
        public float GreeningSurface { get; set; }

        /// <summary>
        /// Gets or sets the list of land rents where the destination farm is this one.
        /// </summary>
        public List<LandRentDTO>? RentedInLands { get; set; } = new List<LandRentDTO>();
    }
    /// <summary>
    /// Represents a data transfer object (DTO) for livestock data,
    /// including the number of animals, dairy cows, milk production, and related costs.
    /// </summary>
    [NotMapped]
    public class LivestockDTO
    {
        /// <summary>
        /// Gets or sets the number of animals in the livestock.
        /// </summary>
        public float NumberOfAnimals { get; set; }

        /// <summary>
        /// Gets or sets the number of dairy cows in the livestock.
        /// </summary>
        public int DairyCows { get; set; }

        /// <summary>
        /// Gets or sets the number of rebreeding cows in the livestock.
        /// </summary>
        public float RebreedingCows { get; set; }

        /// <summary>
        /// Gets or sets the quantity of milk produced in tons.
        /// </summary>
        public float MilkProduction { get; set; }

        /// <summary>
        /// Gets or sets the unit selling price of milk in euros per ton.
        /// </summary>
        public float MilkSellingPrice { get; set; }

        /// <summary>
        /// Gets or sets the variable costs per produced unit of milk in euros per ton.
        /// </summary>
        public float VariableCosts { get; set; }
    }
    /// <summary>
    /// Represents a data transfer object (DTO) for holder information,
    /// including age, successors, family members, and gender.
    /// </summary>
    [NotMapped]
    public class HolderInfoDTO
    {
        /// <summary>
        /// Gets or sets the age of the holder.
        /// </summary>
        public int HolderAge { get; set; }

        /// <summary>
        /// Gets or sets the age of the holder's successors.
        /// </summary>
        public int HolderSuccessorsAge { get; set; }

        /// <summary>
        /// Gets or sets the number of holder's successors.
        /// </summary>
        public long HolderSuccessors { get; set; }

        /// <summary>
        /// Gets or sets the number of family members of the holder.
        /// </summary>
        public int HolderFamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the gender of the holder.
        /// </summary>
        public string HolderGender { get; set; } = "";
    }


    /// <summary>
    /// Represents a data transfer object (DTO) for crop data,
    /// including productive area, variable costs, quantity sold, and unit selling price.
    /// </summary>
    [NotMapped]
    public class CropDataDTO
    {
        /// <summary>
        /// Gets or sets the crop productive area in hectares.
        /// </summary>
        public float CropProductiveArea { get; set; }

        /// <summary>
        /// Gets or sets the variable costs per produced unit in euros per ton.
        /// </summary>
        public float CropVariableCosts { get; set; }

        /// <summary>
        /// Gets or sets the quantity of sold production in tons.
        /// </summary>
        public float QuantitySold { get; set; }

        /// <summary>
        /// Gets or sets the quantity of used production in tons.
        /// </summary>
        public float QuantityUsed { get; set; }

        /// <summary>
        /// Gets or sets the total value of coupled subsidy received in euros.
        /// Deprecated: To be removed.
        /// </summary>
        public float CoupledSubsidy { get; set; }

        /// <summary>
        /// Gets or sets the used area in hectares.
        /// </summary>
        public float? UAA { get; set; }

        /// <summary>
        /// Gets or sets the unit selling price in euros per unit.
        /// </summary>
        public float? CropSellingPrice { get; set; }

        /// <summary>
        /// Gets or sets the number of rebreeding cows in LSU (Livestock Unit).
        /// </summary>
        public float? RebreedingCows { get; set; }

        /// <summary>
        /// Gets or sets the number of dairy cows in LSU (Livestock Unit).
        /// </summary>
        public float? DairyCows { get; set; }
    }

}
