using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace DB.Data.AutoMapper
{
    /// <summary>
    /// Provides mapping configurations for data transfer objects.
    /// </summary>
    internal class AutoMapping : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapping"/> class.
        /// </summary>
        public AutoMapping()
        {

            CreateMap<LandTransaction,LandTransactionJsonDTO>()
                .ForMember(dest => dest.YearNumber , opt => opt.MapFrom(src => src.Year.YearNumber))
                .ForMember(dest => dest.OriginFarmCode, opt => opt.MapFrom(src => src.Production.Farm.FarmCode))
                .ForMember(dest => dest.DestinationFarmCode, opt => opt.MapFrom(src => src.DestinationFarm.FarmCode))
                .ForMember(dest => dest.ProductGroupName, opt => opt.MapFrom(src => src.Production.ProductGroup.Name));

            CreateMap<LandTransactionJsonDTO, LandTransaction>();
            CreateMap<LandRent, LandRentJsonDTO>()
                .ForMember(dest => dest.YearNumber, opt => opt.MapFrom(src => src.Year.YearNumber))
                .ForMember(dest => dest.OriginFarmCode, opt => opt.MapFrom(src => src.OriginFarm.FarmCode))
                .ForMember(dest => dest.DestinationFarmCode, opt => opt.MapFrom(src => src.DestinationFarm.FarmCode));

            CreateMap<LandRentJsonDTO, LandRent>();
            CreateMap<LandRent, LandRentDTO>().
                ForMember(dest => dest.OriginFarmId, opt => opt.MapFrom(src => src.OriginFarm.Id)).
                ForMember(dest => dest.DestinationFarmId, opt => opt.MapFrom(src => src.DestinationFarm.Id));

            CreateMap<AgroManagementDecision, AgroManagementDecisionJsonDTO>()
                .ForMember(dest => dest.YearNumber, opt => opt.MapFrom(src => src.Year.YearNumber));
            CreateMap<AgroManagementDecisionJsonDTO, AgroManagementDecision>();

            CreateMap<ClosingValFarmValueDTO, ClosingValFarmValue>();
            CreateMap<ClosingValFarmValue, ClosingValFarmValueDTO>();

            CreateMap<FarmYearSubsidy, FarmYearSubsidyDTO>()
                .ForMember(dest => dest.YearNumber, opt => opt.MapFrom(src => src.Year == null ? 0 : src.Year.YearNumber))
                .ForMember(dest => dest.PolicyIdentifier, opt => opt.MapFrom(src => src.Policy == null ? "" : src.Policy.PolicyIdentifier));
            CreateMap<FarmYearSubsidyDTO, FarmYearSubsidy>();

            CreateMap<LandTransaction, LandTransactionDTO>();
            CreateMap<LandTransactionDTO, LandTransaction>()
                .ForMember(dest => dest.DestinationFarm, opt => opt.Ignore())
                .ForMember(dest => dest.Production, opt => opt.Ignore())
                .ForMember(dest => dest.Year, opt => opt.Ignore());

            CreateMap<AgroManagementDecisionDTO, AgroManagementDecision>()
                .ForMember(dest => dest.Farm, opt => opt.Ignore())
                .ForMember(dest => dest.Year, opt => opt.Ignore());
            CreateMap<AgroManagementDecision, AgroManagementDecisionDTO>();

            CreateMap<AgriculturalProduction,AgriCulturalProductionDTO>();
            CreateMap<AgriCulturalProductionDTO, AgriculturalProduction>()
                .ForMember(dest => dest.Farm, opt => opt.Ignore())
                .ForMember(dest => dest.Year, opt => opt.Ignore())
                .ForMember(dest => dest.ProductGroup, opt => opt.Ignore());

            CreateMap<HolderFarmYearDataCreateDTO, HolderFarmYearData>();

            CreateMap<SyntheticPopulation, SyntheticPopulationJsonDTO>();

            CreateMap<SyntheticPopulationJsonDTO, SyntheticPopulation>();

            CreateMap<Population, PopulationJsonDTO>();

            CreateMap<PopulationJsonDTO, Population>();

            CreateMap<Farm, FarmJsonDTO>()
                .ForMember(dest => dest.Altitude, opt => opt.MapFrom(src => src.Altitude.ToString()));

            CreateMap<FarmJsonDTO, Farm>();

            CreateMap<AgriculturalProduction, AgriculturalProductionJsonDTO>()
                .ForMember(dest => dest.OrganicProductionType, opt => opt.MapFrom(src => src.OrganicProductionType.ToString()));

            CreateMap<AgriculturalProductionJsonDTO, AgriculturalProduction>();

            CreateMap<LivestockProduction, LivestockProductionJsonDTO>();

            CreateMap<LivestockProductionJsonDTO, LivestockProduction>();

            CreateMap<ProductGroup, ProductGroupJsonDTO>()
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()));

            CreateMap<ProductGroupJsonDTO, ProductGroup>();

            CreateMap<Policy, PolicyJsonDTO>()
                .ForMember(dest => dest.PolicyIdentifier, opt => opt.MapFrom(src => src.PolicyIdentifier == null ? "" : src.PolicyIdentifier));

            CreateMap<PolicyJsonDTO, Policy>();

            CreateMap<PolicyGroupRelation, PolicyGroupRelationJsonDTO>()
                .ForMember(dest => dest.PolicyIdentifier, opt => opt.MapFrom(src => (src.Policy == null || src.Policy.PolicyIdentifier==null) ? "" : src.Policy.PolicyIdentifier))
                .ForMember(dest => dest.ProductGroupName, opt => opt.MapFrom(src => (src.ProductGroup == null) ? "" : src.ProductGroup.Name));

            CreateMap<PolicyGroupRelationJsonDTO, PolicyGroupRelation>();

            CreateMap<FADNProduct, FADNProductJsonDTO>()
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()));

            CreateMap<FADNProductJsonDTO, FADNProduct>();

            CreateMap<HolderFarmYearData, HolderFarmYearDataJsonDTO>()
                .ForMember(dest => dest.HolderGender, opt => opt.MapFrom(src => src.HolderGender.ToString()))
                .ForMember(dest => dest.YearNumber, opt => opt.MapFrom(src => src.Year == null ? 0 : src.Year.YearNumber));

            CreateMap<HolderFarmYearDataJsonDTO, HolderFarmYearData>();

            CreateMap<GreeningFarmYearData, GreeningFarmYearDataJsonDTO>()
                .ForMember(dest => dest.YearNumber, opt => opt.MapFrom(src => src.Year == null ? 0 : src.Year.YearNumber));
            CreateMap<GreeningFarmYearDataJsonDTO, GreeningFarmYearData>();

            CreateMap<SimulationScenario, SimulationScenarioAddDTO>()
                .ForMember(dest => dest.QueueSuffix, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => (src.AdditionalPolicies == null || src.AdditionalPolicies == "" || src.AdditionalPolicies == "{}") ? new List<PolicyForUIDTO>() : JsonConvert.DeserializeObject<List<PolicyForUIDTO>>(src.AdditionalPolicies)));
            CreateMap<SimulationScenarioAddDTO, SimulationScenario>()
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.AdditionalPolicies)));
            CreateMap<SimulationScenario, SimulationScenarioWithIdDTO>()
                .ForMember(dest => dest.QueueSuffix, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => (src.AdditionalPolicies == null || src.AdditionalPolicies == "" || src.AdditionalPolicies == "{}") ? new List<PolicyForUIDTO>() : JsonConvert.DeserializeObject<List<PolicyForUIDTO>>(src.AdditionalPolicies)));
            CreateMap<SimulationScenarioWithIdDTO, SimulationScenario>()
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.AdditionalPolicies)));
            CreateMap<SimulationScenario, SimulationScenarioWithScenarioRunDTO>()
                .ForMember(dest => dest.QueueSuffix, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => (src.AdditionalPolicies == null || src.AdditionalPolicies == "" || src.AdditionalPolicies == "{}") ? new List<PolicyForUIDTO>() : JsonConvert.DeserializeObject<List<PolicyForUIDTO>>(src.AdditionalPolicies)));
            CreateMap<SimulationScenarioWithScenarioRunDTO, SimulationScenario>()
                .ForMember(dest => dest.AdditionalPolicies, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.AdditionalPolicies)));

            CreateMap<SimulationRun, SimulationRunWithIdDTO>();
            CreateMap<SimulationRunWithIdDTO, SimulationRun>();

            CreateMap<Policy, PolicyForUIDTO>()
                .ForMember(dest => dest.CoupledCompensations, opt => opt.MapFrom(src => src.PolicyGroupRelations == null ? new List<CoupledCompensationForUIDTO>() : src.PolicyGroupRelations.Select(pgr => new CoupledCompensationForUIDTO { ProductGroup = pgr.ProductGroup.Name, EconomicCompensation = pgr.EconomicCompensation }).ToList()));
        }
    }

}
