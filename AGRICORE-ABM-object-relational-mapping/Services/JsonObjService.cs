using CsvHelper.Configuration;
using CsvHelper;
using DB.Data.Models;
using DB.Data.Repositories;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DB.Data.DTOs;
using AutoMapper;
using System.Security.Cryptography.Xml;

namespace AGRICORE_ABM_object_relational_mapping.Services
{
    /// <summary>
    /// Service for import export operations for populations and associated data for simulations.
    /// </summary>
    public interface IJsonObjService
    {

        Task<(List<FarmJsonDTO>, Dictionary<string, long>)> ExportFarms(long populationId, List<long> includedYearIds, int? limitFarms = null, long? minimumFarmIdToInclude = null);
        Task<(List<LandRentJsonDTO>, List<LandTransactionJsonDTO>)> ExportTransactions(long populationId, List<long> includedYearIds);
        Task<(List<PolicyJsonDTO>, List<ProductGroupJsonDTO>, List<PolicyGroupRelationJsonDTO>)> ExportPPP(long populationId);

        Task<(SyntheticPopulationJsonDTO, Dictionary<string,long> IncludedFarms)> ExportSyntheticPopulation(long syntheticPopulationId, int? limitFarms = null, long? minimumIdToInclude = null, bool includeFarms = true, bool includePoliciesAndProductGroups = true, bool includeTransactions = true);
        Task<(PopulationJsonDTO, Dictionary<string,long> IncludedFarms)> ExportPopulation(long populationId, List<long> includedYearIds, int? limitFarms = null, long? minimumFarmIdToInclude = null, bool includeFarms = true, bool includePoliciesAndProductGroups = true, bool includeTransactions = true);
        Task<long> ImportPopulationFromSPJson(SyntheticPopulationJsonDTO spjson, bool fixIncomeAndGM = true);
        Task<long> ImportPopulationFromJson(PopulationJsonDTO pjson, bool fixNetIncomeAndGM = true);
        Task<(bool,string?)> ImportPartialFarmsIntoPopulationFromJson(long populationId, List<FarmJsonDTO> farms, Population reusedPopulationObject = null);
        Task<(bool, string?)> FixNetIncomeAndGrossMarginCalculation(List<long> farmIds, List<long> limitToYears);
        
        Task<(bool, string?)> ImportLandRentsFromJson(long newPopulationId, List<LandRentJsonDTO> landRents, Dictionary<string, long>? farmCodeDictionary = null);
        Task<(bool, string?)> ImportLandTransactionsFromJson(long newPopulationId, List<LandTransactionJsonDTO> landTransactions, Dictionary<string, long>? farmCodeDictionary = null);

    }
    /// <summary>
    /// Provides operations for importing and exporting synthetic populations and their related data, such as farms, policies, product groups, and transactions.
    /// </summary>
    public class JsonObjService : IJsonObjService
    {
        private readonly IRepository<SyntheticPopulation> _syntheticPopulationRepository;
        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<FADNProductRelation> _repositoryFADNProductRelation;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IRepository<LandTransaction> _repositoryLandTransaction;
        private readonly IRepository<ClosingValFarmValue> _repositoryClosingValFarmValue;
        private readonly IRepository<LandRent> _repositoryLandRent;
        private readonly IRepository<AgriculturalProduction> _repositoryAgriculturalProduction;
        private readonly IMapper _mapper;
        private readonly ILogger<JsonObjService> _logger;

        public JsonObjService(IRepository<SyntheticPopulation> syntheticPopulationRepository, IRepository<Population> repositoryPopulation, 
            IMapper mapper, IRepository<FADNProduct> repositoryFADNProduct,
            IRepository<Year> repositoryYear, IRepository<Policy> repositoryPolicy, IRepository<FADNProductRelation> repositoryFADNProductRelation, 
            IRepository<Farm> repositoryFarm, IRepository<LandTransaction> repositoryLandTransaction, 
            IRepository<LandRent> repositoryLandRent,
            IRepository<ClosingValFarmValue> repositoryClosingValFarmValue,
            IRepository<AgriculturalProduction> repositoryAgriculturalProduction,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation, IRepository<ProductGroup> repositoryProductGroup, ILogger<JsonObjService> logger)
        {
            _syntheticPopulationRepository = syntheticPopulationRepository;
            _repositoryPopulation = repositoryPopulation;
            _mapper = mapper;
            _repositoryFADNProduct = repositoryFADNProduct;
            _repositoryYear = repositoryYear;
            _repositoryPolicy = repositoryPolicy;
            _repositoryFADNProductRelation = repositoryFADNProductRelation;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _repositoryProductGroup = repositoryProductGroup;
            _repositoryFarm = repositoryFarm;
            _repositoryLandTransaction = repositoryLandTransaction;
            _repositoryClosingValFarmValue = repositoryClosingValFarmValue;
            _repositoryLandRent = repositoryLandRent;
            _repositoryAgriculturalProduction = repositoryAgriculturalProduction;
            _logger = logger;
        }

        /// <summary>
        /// Export a synthetic population along with related data such as farms, policies, product groups, and transactions.
        /// </summary>
        /// <param name="syntheticPopulationId">The ID of the synthetic population to export.</param>
        /// <param name="limitFarms">Optional. The maximum number of farms to include.</param>
        /// <param name="minimumIdToInclude">Optional. The minimum farm ID to include.</param>
        /// <param name="includeFarms">Optional. Indicates whether to include farms in the export (default is true).</param>
        /// <param name="includePoliciesAndProductGroups">Optional. Indicates whether to include policies and product groups in the export (default is true).</param>
        /// <param name="includeTransactions">Optional. Indicates whether to include transactions in the export (default is true).</param>
        /// <returns>
        /// A tuple containing a SyntheticPopulationJsonDTO object representing the exported synthetic population,
        /// and a dictionary mapping farm codes to their corresponding IDs.
        /// </returns>
        public async Task<(SyntheticPopulationJsonDTO, Dictionary<string,long>)> ExportSyntheticPopulation(long syntheticPopulationId, int? limitFarms = null, long? minimumIdToInclude = null, bool includeFarms = true, bool includePoliciesAndProductGroups = true, bool includeTransactions = true)
        {
            var sp = await _syntheticPopulationRepository.GetSingleOrDefaultAsync(sp => sp.Id == syntheticPopulationId);
            string error = string.Empty;
            if (sp == null)
            {
                error = $"There is no synthetic population with id {syntheticPopulationId}";
                _logger.LogError(error);
                return (null, null);
            }

            SyntheticPopulationJsonDTO synthPopJson = _mapper.Map<SyntheticPopulationJsonDTO>(sp);
            Year year = await _repositoryYear.GetSingleOrDefaultAsync(y => y.Id == sp.YearId) ?? new Year();
            if(year == null)
            {
                error = $"There is no year with id {sp.YearId}";
                _logger.LogError(error);
                return (null, null);
            }
            synthPopJson.YearNumber = year.YearNumber;
            (synthPopJson.Population, var dictionary) = await ExportPopulation(
                sp.PopulationId, 
                includedYearIds: new List<long> { year.Id },
                limitFarms: limitFarms,
                minimumFarmIdToInclude: minimumIdToInclude,
                includeFarms: includeFarms,
                includePoliciesAndProductGroups: includePoliciesAndProductGroups,
                includeTransactions: includeTransactions
                );
            return (synthPopJson, dictionary);
        }

        /// <summary>
        /// Export policies, product groups, and their relations for a given population.
        /// </summary>
        /// <param name="populationId">The ID of the population to export policies and product groups from.</param>
        /// <returns>
        /// A tuple containing lists of PolicyJsonDTO, ProductGroupJsonDTO, and PolicyGroupRelationJsonDTO objects representing
        /// the exported policies, product groups, and their relations respectively.
        /// </returns>

        public async Task<(List<PolicyJsonDTO>, List<ProductGroupJsonDTO>, List<PolicyGroupRelationJsonDTO>)> ExportPPP (long populationId)
        {
            Population population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId,
                    include: p => p
                    .Include(p => p.ProductGroups).ThenInclude(pg => pg.FADNProductRelations).ThenInclude(fpr => fpr.FADNProduct)
                    .Include(p => p.Policies)
                    .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.Policy)
                    .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.ProductGroup)
                    , asSeparateQuery: true
                    , asNoTracking: true
                    ) ?? new Population();

            List<Policy> policies = population.Policies ?? new List<Policy>();
            List<PolicyJsonDTO> policiesJson = _mapper.Map<List<PolicyJsonDTO>>(policies);

            List<ProductGroup> productGroups = population.ProductGroups ?? new List<ProductGroup>();
            List<ProductGroupJsonDTO> productGroupsJson = new List<ProductGroupJsonDTO>();
            foreach (ProductGroup productGroup in productGroups)
            {
                ProductGroupJsonDTO productGroupjson = _mapper.Map<ProductGroupJsonDTO>(productGroup);
                List<FADNProductRelation> fADNProductRelations = productGroup.FADNProductRelations ?? new List<FADNProductRelation>();
                foreach (FADNProductRelation fadnRelation in fADNProductRelations)
                {
                    FADNProduct fadnProduct = fadnRelation.FADNProduct ?? new FADNProduct();
                    FADNProductJsonDTO fadnProductDTO = _mapper.Map<FADNProductJsonDTO>(fadnProduct);
                    fadnProductDTO.RepresentativenessOcurrence = fadnRelation.RepresentativenessOcurrence;
                    fadnProductDTO.RepresentativenessArea = fadnRelation.RepresentativenessArea;
                    fadnProductDTO.RepresentativenessValue = fadnRelation.RepresentativenessValue;

                    productGroupjson.FADNProducts.Add(fadnProductDTO);
                }

                productGroupsJson.Add(productGroupjson);
            }

            List<PolicyGroupRelation> policyGroupRelations = population.PolicyGroupRelations ?? new List<PolicyGroupRelation>();
            List<PolicyGroupRelationJsonDTO> policyGroupRelationsJson = new List<PolicyGroupRelationJsonDTO>();
            foreach (PolicyGroupRelation policyGroupRelation in policyGroupRelations)
            {
                PolicyGroupRelationJsonDTO policyGroupRelationJson = _mapper.Map<PolicyGroupRelationJsonDTO>(policyGroupRelation);
                policyGroupRelationJson.ProductGroupName = policyGroupRelation.ProductGroup.Name;
                policyGroupRelationJson.PolicyIdentifier = policyGroupRelation.Policy.PolicyIdentifier;
                policyGroupRelationsJson.Add(policyGroupRelationJson);
            }

            return (policiesJson, productGroupsJson, policyGroupRelationsJson);
        }

        /// <summary>
        /// Export land rents and transactions for a given population and included years.
        /// </summary>
        /// <param name="populationId">The ID of the population to export transactions from.</param>
        /// <param name="includedYearIds">List of year IDs to include transactions for.</param>
        /// <returns>
        /// A tuple containing lists of LandRentJsonDTO and LandTransactionJsonDTO objects representing
        /// the exported land rents and transactions respectively.
        /// </returns>
        public async Task<(List<LandRentJsonDTO>, List<LandTransactionJsonDTO>)> ExportTransactions(long populationId, List<long> includedYearIds)
        {
            List<Farm> farms = await _repositoryFarm.GetAllAsync(f => f.PopulationId == populationId,
                include: p => p
                    .Include(f => f.LandTransactions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(l => l.Production).ThenInclude(ap => ap.ProductGroup)
                    .Include(f => f.LandTransactions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(l => l.Year)
                    .Include(f => f.LandTransactions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(l => l.Production).ThenInclude(ap => ap.Farm)
                    .Include(f => f.LandInRents.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.Year)
                    .Include(f => f.LandInRents.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.DestinationFarm)
                    .Include(f => f.LandOutRents.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.Year)
                    .Include(f => f.LandOutRents.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.OriginFarm)
                    ,
                    asNoTracking: true,
                    asSeparateQuery: false
                    );

            var landTransactionsJson = new List<LandTransactionJsonDTO>();
            var landRentsJson = new List<LandRentJsonDTO>();

            foreach (Farm farm in farms)
            {
                landTransactionsJson.AddRange(farm.LandTransactions?.Select(lt => _mapper.Map<LandTransactionJsonDTO>(lt)).ToList() ?? new List<LandTransactionJsonDTO>());
                landRentsJson.AddRange(farm.LandInRents?.Select(lt => _mapper.Map<LandRentJsonDTO>(lt)).ToList() ?? new List<LandRentJsonDTO>());
            }

            return (landRentsJson, landTransactionsJson);
        }

        /// <summary>
        /// Export farms, including agricultural and livestock productions, subsidies, agro-management decisions, and other related data.
        /// </summary>
        /// <param name="populationId">The ID of the population to export farms from.</param>
        /// <param name="includedYearIds">List of year IDs to include farm data for.</param>
        /// <param name="limitFarms">Optional. The maximum number of farms to include.</param>
        /// <param name="minimumFarmIdToInclude">Optional. The minimum farm ID to include.</param>
        /// <returns>
        /// A tuple containing a list of FarmJsonDTO objects representing the exported farms,
        /// and a dictionary mapping farm codes to their corresponding IDs.
        /// </returns>
        public async Task<(List<FarmJsonDTO>, Dictionary<string,long>)> ExportFarms(long populationId, List<long> includedYearIds, int? limitFarms = null, long? minimumFarmIdToInclude = null)
        {
            List<Farm> farms = (await _repositoryFarm.GetAllAsync(f => f.PopulationId == populationId && f.Id > (minimumFarmIdToInclude ?? 0),
                include: p => p
                .Include(f => f.AgriculturalProductions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.ProductGroup)
                .Include(f => f.AgriculturalProductions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.Year)
                .Include(f => f.LivestockProductions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(l => l.ProductGroup)
                .Include(f => f.LivestockProductions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(l => l.Year)
                .Include(f => f.HoldersFarmYearData.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(h => h.Year)
                .Include(f => f.GreeningFarmYearData.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(h => h.Year)
                .Include(f => f.ClosingValFarmValues.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(c => c.Year)
                .Include(f => f.FarmYearSubsidies.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(c => c.Year)
                .Include(f => f.FarmYearSubsidies.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(c => c.Policy)
                .Include(f => f.AgroManagementDecisions.Where(q => includedYearIds.Contains(q.YearId))).ThenInclude(a => a.Year)
                ,
                asNoTracking: true,
                asSeparateQuery: true,
                take: limitFarms,
                orderBy: p => p.OrderBy(q => q.Id)
                //debugQuery: true
                )).ToList();

            Dictionary<string,long> farmCodeDictionary = farms.ToDictionary(f => f.FarmCode, f => f.Id);

            List<FarmJsonDTO> farmsJson = new List<FarmJsonDTO>();

            foreach (Farm farm in farms)
            {
                FarmJsonDTO farmDTO = _mapper.Map<FarmJsonDTO>(farm);
                List<AgriculturalProduction> agroProductions = farm.AgriculturalProductions ?? new List<AgriculturalProduction>();
                List<LivestockProduction> livestockProductions = farm.LivestockProductions ?? new List<LivestockProduction>();
                List<HolderFarmYearData> holderData = farm.HoldersFarmYearData ?? new List<HolderFarmYearData>();
                List<GreeningFarmYearData> greeningData = farm.GreeningFarmYearData ?? new List<GreeningFarmYearData>();
                List<ClosingValFarmValue> closingValFarmValue = farm.ClosingValFarmValues ?? new List<ClosingValFarmValue>();
                List<FarmYearSubsidy> farmYearSubsidy = farm.FarmYearSubsidies ?? new List<FarmYearSubsidy>();
                List<LandTransaction> landTransaction = farm.LandTransactions ?? new List<LandTransaction>();
                List<AgroManagementDecision> agroManagementDecisions = farm.AgroManagementDecisions ?? new List<AgroManagementDecision>();

                farmDTO.AgroManagementDecisions = new List<AgroManagementDecisionJsonDTO>();
                agroManagementDecisions.ForEach(q => farmDTO.AgroManagementDecisions.Add(_mapper.Map<AgroManagementDecisionJsonDTO>(q)));

                farmDTO.AgriculturalProductions = new List<AgriculturalProductionJsonDTO>();
                agroProductions.ForEach(q =>
                {
                    var ap = _mapper.Map<AgriculturalProductionJsonDTO>(q);
                    ap.ProductName = q.ProductGroup.Name;
                    ap.YearNumber = q.Year.YearNumber;
                    farmDTO.AgriculturalProductions.Add(ap);
                });

                farmDTO.LivestockProductions = new List<LivestockProductionJsonDTO>();
                livestockProductions.ForEach(q =>
                {
                    var lsp = _mapper.Map<LivestockProductionJsonDTO>(q);
                    lsp.ProductName = q.ProductGroup.Name;
                    lsp.YearNumber = q.Year.YearNumber;
                    farmDTO.LivestockProductions.Add(lsp);
                });

                farmDTO.HolderFarmYearsData = new List<HolderFarmYearDataJsonDTO>();
                holderData.ForEach(q =>
                {
                    var hfd = _mapper.Map<HolderFarmYearDataJsonDTO>(q);
                    hfd.YearNumber = q.Year.YearNumber;
                    farmDTO.HolderFarmYearsData.Add(hfd);
                });
                farmDTO.GreeningFarmYearData = new List<GreeningFarmYearDataJsonDTO>();
                greeningData.ForEach(q =>
                {
                    var gd = _mapper.Map<GreeningFarmYearDataJsonDTO>(q);
                    gd.YearNumber = q.Year.YearNumber;
                    farmDTO.GreeningFarmYearData.Add(gd);
                });

                farmDTO.ClosingValFarmValues = new List<ClosingValFarmValueDTO>();
                closingValFarmValue.ForEach(q =>
                {
                    var cvfv = _mapper.Map<ClosingValFarmValueDTO>(q);
                    cvfv.YearNumber = q.Year.YearNumber;
                    farmDTO.ClosingValFarmValues.Add(cvfv);
                });

                farmDTO.FarmYearSubsidies = new List<FarmYearSubsidyDTO>();
                farmYearSubsidy.ForEach(q =>
                {
                    var fys = _mapper.Map<FarmYearSubsidyDTO>(q);
                    fys.FarmId = farm.Id;
                    farmDTO.FarmYearSubsidies.Add(fys);
                });
                farmsJson.Add(farmDTO);
            }

            return (farmsJson, farmCodeDictionary);
        }

        /// <summary>
        /// Export a population along with related data such as farms, policies, product groups, and transactions.
        /// </summary>
        /// <param name="populationId">The ID of the population to export.</param>
        /// <param name="includedYearIds">List of year IDs to include in the export.</param>
        /// <param name="limitFarms">Optional. The maximum number of farms to include.</param>
        /// <param name="minimumFarmIdToInclude">Optional. The minimum farm ID to include.</param>
        /// <param name="includeFarms">Optional. Indicates whether to include farms in the export (default is true).</param>
        /// <param name="includePoliciesAndProductGroups">Optional. Indicates whether to include policies and product groups in the export (default is true).</param>
        /// <param name="includeTransactions">Optional. Indicates whether to include transactions in the export (default is true).</param>
        /// <returns>
        /// A tuple containing a PopulationJsonDTO object representing the exported population,
        /// and a dictionary mapping farm codes to their corresponding IDs.
        /// </returns>
        public async Task<(PopulationJsonDTO, Dictionary<string,long>)> ExportPopulation(long populationId, List<long> includedYearIds, int? limitFarms = null, long? minimumFarmIdToInclude = null, bool includeFarms = true, bool includePoliciesAndProductGroups = true, bool includeTransactions = true)
        {
            var pop = await _repositoryPopulation.GetSingleOrDefaultAsync(sp => sp.Id == populationId, asNoTracking: true);
            string error = string.Empty;
            if (pop == null)
            {
                error = $"There is no population with id {populationId}";
                _logger.LogError(error);
                return (null,null);
            }

            PopulationJsonDTO popJson = _mapper.Map<PopulationJsonDTO>(pop);

            Dictionary<string, long> farmDictionary = new Dictionary<string, long>();

            if (includeFarms)
            {
                (var farms, farmDictionary) = await ExportFarms(populationId, includedYearIds, limitFarms, minimumFarmIdToInclude);
                popJson.Farms.AddRange(farms);
            }

            if (includeTransactions)
            {
                (var landRentsJson, var landTransactionsJson) = await ExportTransactions(populationId, includedYearIds);
                popJson.LandTransactions = landTransactionsJson;
                popJson.LandRents = landRentsJson;
            }

            if (includePoliciesAndProductGroups)
            {
                (var policiesJson, var productGroupsJson, var policyGroupRelationsJson) = await ExportPPP(populationId);
                popJson.Policies = policiesJson;
                popJson.ProductGroups = productGroupsJson;
                popJson.PolicyGroupRelations = policyGroupRelationsJson;
            }
            
            return (popJson,farmDictionary);
        }

        /// <summary>
        /// Imports a synthetic population from JSON data into the database.
        /// </summary>
        /// <param name="spjson">The SyntheticPopulationJsonDTO containing the synthetic population data.</param>
        /// <param name="fixIncomeAndGM">Optional. Indicates whether to fix net income and gross margin calculations after import (default is true).</param>
        /// <returns>The ID of the newly imported population.</returns>

        public async Task<long> ImportPopulationFromSPJson(SyntheticPopulationJsonDTO spjson, bool fixIncomeAndGM = true)
        {
            long id = 0;
            
            Population newPopulation = await GetPopulationFromSyntheticJson(spjson);
            //if there is no year then no year-related entity has been inserted, only common entities like productgroups, products or policies have been inserted
            //so the synthetic population year is inserted here
            if(newPopulation.Years.Count == 0)
            {
                Year year = new Year
                {
                    YearNumber = spjson.YearNumber,
                };
                newPopulation.Years.Add(year);
            }

            if (newPopulation != null)
            {
                (bool result, var message) = await _repositoryPopulation.AddAsync(newPopulation);
                if (result)
                {
                    if (fixIncomeAndGM)
                    {
                        (result, message) = await FixNetIncomeAndGrossMarginCalculation(newPopulation.Farms.Select(f => f.Id).ToList(), new List<long> { spjson.YearNumber });
                    }
                    if (result)
                    {
                        id = newPopulation.Id;
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Imports farms data into a population object from FarmJsonDTOs.
        /// </summary>
        /// <param name="population">The Population object to import farms into.</param>
        /// <param name="farms">List of FarmJsonDTOs containing the farms data to import.</param>
        /// <returns>The updated Population object with imported farms data.</returns>

        private async Task<Population> ImportPopulationFarms(Population population, List<FarmJsonDTO> farms)
        {
            var existingPolicies = population.Policies;
            foreach (FarmJsonDTO farmJson in farms)
            {
                Farm newFarm = _mapper.Map<Farm>(farmJson);

                newFarm.AgroManagementDecisions = new List<AgroManagementDecision>();
                farmJson.AgroManagementDecisions.ForEach(q =>
                {
                    var amd = _mapper.Map<AgroManagementDecision>(q);
                    amd.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    newFarm.AgroManagementDecisions.Add(amd);
                });
                newFarm.ClosingValFarmValues = new List<ClosingValFarmValue>();
                farmJson.ClosingValFarmValues.ForEach(q =>
                {
                    var cvfv = _mapper.Map<ClosingValFarmValue>(q);
                    cvfv.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    newFarm.ClosingValFarmValues.Add(cvfv);
                });
                newFarm.AgriculturalProductions = new List<AgriculturalProduction>();
                farmJson.AgriculturalProductions.ForEach(q =>
                {
                    var ap = _mapper.Map<AgriculturalProduction>(q);
                    ap.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    ap.ProductGroup = population.ProductGroups.Single(pg => pg.Name == q.ProductName);
                    newFarm.AgriculturalProductions.Add(ap);
                });
                newFarm.HoldersFarmYearData = new List<HolderFarmYearData>();
                farmJson.HolderFarmYearsData.ForEach(q =>
                {
                    var hfyd = _mapper.Map<HolderFarmYearData>(q);
                    hfyd.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    newFarm.HoldersFarmYearData.Add(hfyd);
                });
                newFarm.GreeningFarmYearData = new List<GreeningFarmYearData>();
                farmJson.GreeningFarmYearData.ForEach(q =>
                {
                    var gd = _mapper.Map<GreeningFarmYearData>(q);
                    gd.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    newFarm.GreeningFarmYearData.Add(gd);
                });
                newFarm.LivestockProductions = new List<LivestockProduction>();
                farmJson.LivestockProductions.ForEach(q =>
                {
                    var lsp = _mapper.Map<LivestockProduction>(q);
                    lsp.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    lsp.ProductGroup = population.ProductGroups.Single(pg => pg.Name == q.ProductName);
                    newFarm.LivestockProductions.Add(lsp);
                });
                newFarm.FarmYearSubsidies = new List<FarmYearSubsidy>();
                farmJson.FarmYearSubsidies.ForEach(async q =>
                {
                    var fys = _mapper.Map<FarmYearSubsidy>(q);
                    fys.Year = population.Years.SingleOrDefault(t => t.YearNumber == q.YearNumber);
                    fys.Policy = null;
                    var existingPolicy = existingPolicies.SingleOrDefault(p => p.PolicyIdentifier == q.PolicyIdentifier);
                    if (existingPolicy != null)
                    {
                        fys.Policy = existingPolicy;
                        newFarm.FarmYearSubsidies.Add(fys);
                    } else
                    {
                        // Subsidies for policies not included in the population are ignored
                    }
                });
                newFarm.LandTransactions = new List<LandTransaction>();
                newFarm.LandInRents = new List<LandRent>();
                newFarm.LandOutRents = new List<LandRent>();
                population.Farms.Add(newFarm);
            };

            return population;
        }

        /// <summary>
        /// Imports land transactions and rents into a population object from provided JSON data.
        /// </summary>
        /// <param name="population">The Population object to import land operations into.</param>
        /// <param name="transactions">List of LandTransactionJsonDTOs containing transaction data to import.</param>
        /// <param name="rents">List of LandRentJsonDTOs containing rent data to import.</param>
        /// <returns>The updated Population object with imported land operations data.</returns>

        private async Task<Population> ImportPopulationLandOperations(Population population, List<LandTransactionJsonDTO> transactions, List<LandRentJsonDTO> rents)
        {
            foreach (LandTransactionJsonDTO transaction in transactions)
            {
                Farm originFarm = population.Farms.Find(q => q.FarmCode.Equals(transaction.OriginFarmCode));
                Farm destinationFarm = population.Farms.Find(q => q.FarmCode.Equals(transaction.DestinationFarmCode));
                LandTransaction newTransaction = _mapper.Map<LandTransaction>(transaction);
                newTransaction.Year = population.Years.SingleOrDefault(t => t.YearNumber == transaction.YearNumber);
                newTransaction.Production = originFarm.AgriculturalProductions.Single(agp => agp.ProductGroup.Name == transaction.ProductGroupName && agp.Year.YearNumber == transaction.YearNumber);
                newTransaction.DestinationFarm = destinationFarm;
                originFarm.LandTransactions.Add(newTransaction);
            }
            foreach (LandRentJsonDTO rent in rents)
            {
                Farm originFarm = population.Farms.Find(q => q.FarmCode.Equals(rent.OriginFarmCode));
                Farm destinationFarm = population.Farms.Find(q => q.FarmCode.Equals(rent.DestinationFarmCode));
                LandRent newRent = _mapper.Map<LandRent>(rent);
                newRent.Year = population.Years.SingleOrDefault(t => t.YearNumber == rent.YearNumber);
                newRent.OriginFarm = originFarm;
                newRent.DestinationFarm = destinationFarm;
                if (originFarm != null)
                {
                    if (originFarm.LandOutRents == null)
                    {
                        originFarm.LandOutRents = new List<LandRent>();
                    }
                    originFarm.LandOutRents.Add(newRent);
                } else
                {
                    _logger.LogWarning($"Origin farm {rent.OriginFarmCode} not found for rent {rent.OriginFarmCode} ==> {rent.DestinationFarmCode}: {rent.RentArea} ha for {rent.RentValue}");
                }
            }

            return population;
        }

        /// <summary>
        /// Imports partial farms data into an existing or new population object from JSON data.
        /// </summary>
        /// <param name="populationId">The ID of the population to import farms into.</param>
        /// <param name="farms">List of FarmJsonDTOs containing the farms data to import.</param>
        /// <param name="reusedPopulationObject">Optional. An existing Population object to reuse or null to fetch from repository.</param>
        /// <returns>
        /// A tuple indicating whether the import was successful (bool) and an optional message (string).
        /// </returns>
        public async Task<(bool,string?)> ImportPartialFarmsIntoPopulationFromJson(long populationId, List<FarmJsonDTO> farms, Population reusedPopulationObject = null)
        {
            Population population = null;
            if (reusedPopulationObject != null)
            {
                population = reusedPopulationObject;
            }
            else
            {
                population = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
                    .Include(p => p.ProductGroups)
                    .Include(p => p.PolicyGroupRelations)
                    .Include(p => p.Policies)
                    .Include(p => p.FADNProductRelations)
                    .Include(p => p.Years)
                    .Include(p => p.ProductGroups),
                    asNoTracking: true,
                    asSeparateQuery: true
                );
            }

            List<Farm> newFarms = new List<Farm>();

            var existingPolicies = population.Policies;
            var yearNumberToYearIdDict = population.Years.Select(y => new { y.Id, y.YearNumber }).ToDictionary(y => y.YearNumber, y => y.Id);
            var pgDict = population.ProductGroups.Select(y => new { y.Id, y.Name }).ToDictionary(y => y.Name, y => y.Id);
            var policyDict = existingPolicies.Select(y => new { y.Id, y.PolicyIdentifier }).ToDictionary(y => y.PolicyIdentifier, y => y.Id);
            foreach (FarmJsonDTO farmJson in farms)
            {
                Farm newFarm = _mapper.Map<Farm>(farmJson);
                newFarm.PopulationId = populationId;

                newFarm.AgroManagementDecisions = new List<AgroManagementDecision>();
                farmJson.AgroManagementDecisions.ForEach(q =>
                {
                    var amd = _mapper.Map<AgroManagementDecision>(q);
                    amd.YearId = yearNumberToYearIdDict[q.YearNumber];
                    newFarm.AgroManagementDecisions.Add(amd);
                });
                newFarm.ClosingValFarmValues = new List<ClosingValFarmValue>();
                farmJson.ClosingValFarmValues.ForEach(q =>
                {
                    var cvfv = _mapper.Map<ClosingValFarmValue>(q);
                    cvfv.YearId = yearNumberToYearIdDict[q.YearNumber];
                    newFarm.ClosingValFarmValues.Add(cvfv);
                });
                newFarm.AgriculturalProductions = new List<AgriculturalProduction>();
                farmJson.AgriculturalProductions.ForEach(q =>
                {
                    var ap = _mapper.Map<AgriculturalProduction>(q);
                    ap.YearId = yearNumberToYearIdDict[q.YearNumber];
                    ap.ProductGroupId = pgDict[q.ProductName];
                    newFarm.AgriculturalProductions.Add(ap);
                });
                newFarm.HoldersFarmYearData = new List<HolderFarmYearData>();
                farmJson.HolderFarmYearsData.ForEach(q =>
                {
                    var hfyd = _mapper.Map<HolderFarmYearData>(q);
                    hfyd.YearId = yearNumberToYearIdDict[q.YearNumber];
                    newFarm.HoldersFarmYearData.Add(hfyd);
                });
                newFarm.GreeningFarmYearData = new List<GreeningFarmYearData>();
                farmJson.GreeningFarmYearData.ForEach(q =>
                {
                    var gd = _mapper.Map<GreeningFarmYearData>(q);
                    gd.YearId = yearNumberToYearIdDict[q.YearNumber];
                    newFarm.GreeningFarmYearData.Add(gd);
                });

                newFarm.LivestockProductions = new List<LivestockProduction>();
                farmJson.LivestockProductions.ForEach(q =>
                {
                    var lsp = _mapper.Map<LivestockProduction>(q);
                    lsp.YearId = yearNumberToYearIdDict[q.YearNumber];
                    lsp.ProductGroupId = pgDict[q.ProductName];
                    newFarm.LivestockProductions.Add(lsp);
                });
                newFarm.FarmYearSubsidies = new List<FarmYearSubsidy>();
                farmJson.FarmYearSubsidies.ForEach(async q =>
                {
                    var fys = _mapper.Map<FarmYearSubsidy>(q);
                    fys.YearId = yearNumberToYearIdDict[q.YearNumber];
                    fys.PolicyId = policyDict[q.PolicyIdentifier];
                    newFarm.FarmYearSubsidies.Add(fys);
                });
                newFarm.LandTransactions = new List<LandTransaction>();
                newFarms.Add(newFarm);
            };

            (bool result, var message) = await _repositoryFarm.AddRangeAsync(newFarms);
            if (result)
            {
                (result, message) = await FixNetIncomeAndGrossMarginCalculation(newFarms.Select(f => f.Id).ToList(), population.Years.Select(y => y.YearNumber).ToList());

            }
            return (result, message);
        }
        /// <summary>
        /// Imports a full population object from JSON data into the database.
        /// </summary>
        /// <param name="pjson">The PopulationJsonDTO containing the population data.</param>
        /// <param name="fixNetIncomeAndGM">Optional. Indicates whether to fix net income and gross margin calculations after import (default is true).</param>
        /// <returns>The ID of the newly imported population.</returns>
        public async Task<long> ImportPopulationFromJson(PopulationJsonDTO pjson, bool fixNetIncomeAndGM = true)
        {
            long id = 0;

            Population newPopulation = await GetPopulationFromJson(pjson);

            if (newPopulation != null)
            {
                (bool result, var message) = await _repositoryPopulation.AddAsync(newPopulation);
                if (result)
                {
                    if (fixNetIncomeAndGM)
                    {
                        (result, message) = await FixNetIncomeAndGrossMarginCalculation(newPopulation.Farms.Select(f => f.Id).ToList(), newPopulation.Years.Select(y => y.YearNumber).ToList());
                    }
                    if (result)
                    {
                        id = newPopulation.Id;
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// Constructs a Population object from synthetic JSON data.
        /// </summary>
        /// <param name="spjson">The SyntheticPopulationJsonDTO containing the synthetic population data.</param>
        /// <returns>The constructed Population object.</returns>
        private async Task<Population> GetPopulationFromSyntheticJson(SyntheticPopulationJsonDTO spjson)
        {
            // Preload required objects

            var newPopulation = await GetPopulationFromJson(spjson.Population);
            
            return newPopulation;
        }

        /// <summary>
        /// Constructs a Population object from JSON data.
        /// </summary>
        /// <param name="pjson">The PopulationJsonDTO containing the population data.</param>
        /// <returns>The constructed Population object.</returns>
        private async Task<Population> GetPopulationFromJson(PopulationJsonDTO pjson)
        {
            // Preload required objects
            var existingFADNProducts = await _repositoryFADNProduct.GetAllAsync(asNoTracking: true);

            //Create related Population
            Population newPopulation = new Population
            {
                Description = pjson.Description,
                Years = new List<Year>(),
                SyntheticPopulations = new List<SyntheticPopulation> { },
                Farms = new List<Farm>(),
                ProductGroups = new List<ProductGroup>(),
                FADNProductRelations = new List<FADNProductRelation>(),
                PolicyGroupRelations = new List<PolicyGroupRelation>(),
                Policies = new List<Policy>(),
                SimulationScenarios = new List<SimulationScenario>()
            };

            // Extract Years
            List<long> yearNumbers = new List<long>();
            if (pjson.Farms != null && pjson.Farms.Count > 0)
            {
                foreach (FarmJsonDTO farmJson in pjson.Farms)
                {
                    yearNumbers.AddRange(farmJson.AgriculturalProductions.Select(ap => ap.YearNumber));
                    yearNumbers.AddRange(farmJson.LivestockProductions.Select(lp => lp.YearNumber));
                    yearNumbers.AddRange(farmJson.HolderFarmYearsData.Select(h => h.YearNumber));
                    yearNumbers.AddRange(farmJson.ClosingValFarmValues.Select(c => c.YearNumber));
                    yearNumbers.AddRange(farmJson.FarmYearSubsidies.Select(f => f.YearNumber));
                    yearNumbers.AddRange(farmJson.GreeningFarmYearData.Select(g => g.YearNumber));
                    yearNumbers.AddRange(farmJson.AgroManagementDecisions.Select(l => l.YearNumber));
                }
            }
            if (pjson.LandRents != null && pjson.LandRents.Count > 0)
            {
                yearNumbers.AddRange(pjson.LandRents.Select(l => l.YearNumber));
            }
            if (pjson.LandTransactions != null && pjson.LandTransactions.Count > 0)
            {
                yearNumbers.AddRange(pjson.LandTransactions.Select(l => l.YearNumber));
            }

            yearNumbers = yearNumbers.Distinct().ToList();
            newPopulation.Years = new List<Year>();

            foreach (var yearNumber in yearNumbers)
            {
                Year newYear = new Year
                {
                    YearNumber = yearNumber,
                    HoldersFarmYearData = new List<HolderFarmYearData>(),
                    GreeningFarmYearData = new List<GreeningFarmYearData>(),
                    SyntheticPopulations = new List<SyntheticPopulation> { },
                    LandTransactions = new List<LandTransaction> { },
                    LandRents = new List<LandRent> { },
                    AgriculturalProductions = new List<AgriculturalProduction> {  },
                    FarmYearSubsidies = new List<FarmYearSubsidy> { },
                    AgroManagementDecisions = new List<AgroManagementDecision> {  },
                    LivestockProductions = new List<LivestockProduction> { },
                    SimulationScenarios = new List<SimulationScenario> { },
                };
                newPopulation.Years.Add(newYear);
            }

            if (pjson.ProductGroups != null && pjson.ProductGroups.Count > 0)
            {
                //Create ProductGroups, FADNProducts and Policies
                foreach (ProductGroupJsonDTO pgJson in pjson.ProductGroups)
                {
                    ProductGroup newPg = _mapper.Map<ProductGroup>(pgJson);
                    newPg.FADNProductRelations = new List<FADNProductRelation>();
                    foreach (FADNProductJsonDTO productJson in pgJson.FADNProducts)
                    {
                        var existingProduct = existingFADNProducts.SingleOrDefault(p => p.FADNIdentifier == productJson.FADNIdentifier);
                        if (existingProduct == null)
                        {
                            FADNProduct newProduct = _mapper.Map<FADNProduct>(productJson);
                            await _repositoryFADNProduct.AddAsync(newProduct);
                            existingProduct = newProduct;
                            existingFADNProducts.Add(newProduct);
                        }
                        var newFadnProductRelation = new FADNProductRelation
                        {
                            FADNProductId = existingProduct.Id,
                            ProductGroup = newPg,
                            RepresentativenessOcurrence = productJson.RepresentativenessOcurrence,
                            RepresentativenessArea = productJson.RepresentativenessArea,
                            RepresentativenessValue = productJson.RepresentativenessValue,
                        };
                        newPopulation.FADNProductRelations.Add(newFadnProductRelation);
                    }
                    newPopulation.ProductGroups.Add(newPg);
                }
            }

            if (pjson.Policies != null && pjson.Policies.Count > 0)
            {
                var processedPolicyCodes = new List<string>();
                foreach (PolicyJsonDTO policyJson in pjson.Policies)
                {
                    if (processedPolicyCodes.Contains(policyJson.PolicyIdentifier))
                    {
                        continue;
                    }
                    else { 
                        Policy newPolicy = _mapper.Map<Policy>(policyJson);
                        newPopulation.Policies.Add(newPolicy);
                        processedPolicyCodes.Add(policyJson.PolicyIdentifier);
                    }
                }

                var processedRelations = new List<(string, string)>();

                foreach (PolicyGroupRelationJsonDTO pgrJson in pjson.PolicyGroupRelations)
                {
                    if (processedRelations.Contains((pgrJson.PolicyIdentifier, pgrJson.ProductGroupName)))
                    {
                        continue;
                    }
                    else
                    {
                        processedRelations.Add((pgrJson.PolicyIdentifier, pgrJson.ProductGroupName));
                    }
                    PolicyGroupRelation newPgr = _mapper.Map<PolicyGroupRelation>(pgrJson);
                    newPgr.Policy = newPopulation.Policies.Single(p => p.PolicyIdentifier == pgrJson.PolicyIdentifier);
                    newPgr.ProductGroup = newPopulation.ProductGroups.Single(pg => pg.Name == pgrJson.ProductGroupName);
                    newPopulation.PolicyGroupRelations.Add(newPgr);
                }
            }
           
            //Add Farms, Holders amd Productions
            if (pjson.Farms!= null && pjson.Farms.Count > 0)
            {
                newPopulation = await ImportPopulationFarms(newPopulation, pjson.Farms);
            }

            if (pjson.LandRents != null && pjson.LandRents.Count > 0)
            {
                newPopulation = await ImportPopulationLandOperations(newPopulation, pjson.LandTransactions, pjson.LandRents);
            }

            return newPopulation;
        }

        /// <summary>
        /// Fixes net income and gross margin calculations for specified farms and years based on their productions, subsidies, and rents.
        /// </summary>
        /// <param name="farmIds">List of farm IDs to calculate net income and gross margin for.</param>
        /// <param name="limitToYears">Optional. List of years to limit calculations to. If null, all available years are considered.</param>
        /// <returns>A tuple indicating whether the operation was successful (bool) and an optional message (string).</returns>

        public async Task<(bool, string?)> FixNetIncomeAndGrossMarginCalculation(List<long> farmIds, List<long> limitToYears = null)
        {
            var farms = await _repositoryFarm.GetAllAsync(
                f => farmIds.Contains(f.Id),
                include: f => f.Include(f => f.AgriculturalProductions).Include(f => f.LivestockProductions).
                Include(f => f.FarmYearSubsidies).
                Include(f => f.LandInRents).Include(f => f.LandOutRents), asNoTracking: true, asSeparateQuery: true);

            var closingValues = await _repositoryClosingValFarmValue.GetAllAsync(
                cv => farmIds.Contains(cv.FarmId),
                include: cv => cv.Include(cv => cv.Year));

            List<Year> includedYears = new List<Year>();
            if (limitToYears == null || limitToYears.Count == 0) {
                includedYears = closingValues.Select(cv => cv.Year).DistinctBy(q => q.YearNumber).ToList();
            }
            else
            {
                includedYears = closingValues.Where(q => limitToYears.Contains(q.Year.YearNumber)).Select(cv => cv.Year).DistinctBy(q => q.YearNumber).ToList();
            }
            var landTransactions = await _repositoryLandTransaction.GetAllAsync(
                lt => includedYears.Select(q => q.Id).Contains(lt.YearId) &&
                    (farmIds.Contains(lt.DestinationFarmId) || farmIds.Contains(lt.Production.FarmId)),
                    include: q => q.Include(q => q.Production), asNoTracking: true);

            List<ClosingValFarmValue> toUpdate = new List<ClosingValFarmValue>();

            foreach (var farm in farms)
            {
                foreach (var year in includedYears)
                {
                    var agriculturalIncome = farm.AgriculturalProductions.Where(ap => ap.YearId == year.Id).Sum(ap => ap.ValueSales);
                    var agriculturalCosts = farm.AgriculturalProductions.Where(ap => ap.YearId == year.Id).Sum(ap => ap.VariableCosts * (ap.QuantitySold + ap.QuantityUsed));
                    var livestockIncome = farm.LivestockProductions.Where(ap => ap.YearId == year.Id).Sum(ap => ap.ManureTotalSales + ap.EggsTotalSales + ap.MilkTotalSales);
                    var livestockCosts = farm.LivestockProductions.Where(ap => ap.YearId == year.Id).Sum(ap => ap.VariableCosts * ap.MilkTotalProduction);
                    var subsidies = farm.FarmYearSubsidies.Where(ap => ap.YearId == year.Id).Sum(ap => ap.Value);
                    var rentBalance = farm.LandOutRents.Where(ap => ap.YearId == year.Id).Sum(ap => ap.RentValue) - farm.LandInRents.Where(ap => ap.YearId == year.Id).Sum(ap => ap.RentValue);
                    var landTransactionBalance = landTransactions.Where(ap => ap.YearId == year.Id && ap.DestinationFarmId == farm.Id).Sum(ap => ap.SalePrice) - landTransactions.Where(ap => ap.YearId == year.Id && ap.Production.FarmId == farm.Id).Sum(ap => ap.SalePrice);

                    var thisClosingValue = closingValues.Single(ap => ap.YearId == year.Id && ap.FarmId == farm.Id);
                    thisClosingValue.GrossFarmIncome =
                        agriculturalIncome ?? 0 + livestockIncome + subsidies;
                    thisClosingValue.FarmNetIncome = thisClosingValue.GrossFarmIncome - agriculturalCosts ?? 0 - livestockCosts +
                        rentBalance + landTransactionBalance;
                    thisClosingValue.Farm = null;
                    thisClosingValue.Year = null;
                    toUpdate.Add(thisClosingValue);
                }
            }
            return await _repositoryClosingValFarmValue.UpdateRangeAsync(toUpdate);
        }

        //TODO: These methods are repeated, we have two ways to import landrents and transfers and would need to be homogenised in the future

        /// <summary>
        /// Imports land rents into the database from JSON data for a new population.
        /// </summary>
        /// <param name="newPopulationId">The ID of the new population to import land rents into.</param>
        /// <param name="landRents">List of LandRentJsonDTOs containing the land rent data to import.</param>
        /// <param name="farmCodeDictionary">Optional. Dictionary mapping farm codes to database IDs.</param>
        /// <returns>A tuple indicating whether the import was successful (bool) and an optional message (string).</returns>

        public async Task<(bool, string?)> ImportLandRentsFromJson(long newPopulationId, List<LandRentJsonDTO> landRents, Dictionary<string, long>? farmCodeDictionary = null)
        {
            List<LandRent> newLandRents = new List<LandRent>();

            List<long> involvedYears = landRents.Select(q => q.YearNumber).Distinct().ToList();
            List<Year> years = await _repositoryYear.GetAllAsync(q => involvedYears.Contains(q.YearNumber) && q.PopulationId == newPopulationId);
            foreach (LandRentJsonDTO landRent in landRents)
            {
                LandRent newLandRent = _mapper.Map<LandRent>(landRent);
                long originFarmId = farmCodeDictionary[landRent.OriginFarmCode];
                long destinationFarmId = farmCodeDictionary[landRent.DestinationFarmCode];
                long yearId = years.Single(q => q.YearNumber == landRent.YearNumber).Id;
                newLandRent.OriginFarmId = originFarmId;
                newLandRent.DestinationFarmId = destinationFarmId;
                newLandRent.YearId = yearId;
                newLandRents.Add(newLandRent);
            }
            (bool result, string? message) = await _repositoryLandRent.AddRangeAsync(newLandRents);
            return (result, message);

        }

        /// <summary>
        /// Imports land transactions into the database from JSON data for a new population.
        /// </summary>
        /// <param name="newPopulationId">The ID of the new population to import land transactions into.</param>
        /// <param name="landTransactions">List of LandTransactionJsonDTOs containing the land transaction data to import.</param>
        /// <param name="farmCodeDictionary">Optional. Dictionary mapping farm codes to database IDs.</param>
        /// <returns>A tuple indicating whether the import was successful (bool) and an optional message (string).</returns>

        public async Task<(bool, string?)> ImportLandTransactionsFromJson(long newPopulationId, List<LandTransactionJsonDTO> landTransactions, Dictionary<string, long>? farmCodeDictionary = null)
        {
            List<LandTransaction> newLandTransactions = new List<LandTransaction>();

            List<long> involvedYears = landTransactions.Select(q => q.YearNumber).Distinct().ToList();
            List<Year> years = await _repositoryYear.GetAllAsync(q => involvedYears.Contains(q.YearNumber) && q.PopulationId == newPopulationId);
            List<long> involvedFarmIds = landTransactions.Select(q => farmCodeDictionary[q.OriginFarmCode]).Distinct().ToList();
            List<AgriculturalProduction> involvedAgriculturalProductions = await _repositoryAgriculturalProduction.GetAllAsync(q => involvedYears.Contains(q.YearId) && involvedFarmIds.Contains(q.FarmId), asSeparateQuery: true, asNoTracking: true);
            foreach (LandTransactionJsonDTO landTransaction in landTransactions)
            {
                LandTransaction newLandTransaction = _mapper.Map<LandTransaction>(landTransaction);
                long originFarmId = farmCodeDictionary[landTransaction.OriginFarmCode];
                long destinationFarmId = farmCodeDictionary[landTransaction.DestinationFarmCode];
                string productGroup = landTransaction.ProductGroupName;
                long originAgriculturalProductionId = involvedAgriculturalProductions.First(q => q.FarmId == farmCodeDictionary[landTransaction.OriginFarmCode] && q.ProductGroup.Name == productGroup).Id;
                long yearId = years.Single(q => q.YearNumber == landTransaction.YearNumber).Id;
                newLandTransaction.ProductionId = originAgriculturalProductionId;
                newLandTransaction.DestinationFarmId = destinationFarmId;
                newLandTransaction.YearId = yearId;
                newLandTransactions.Add(newLandTransaction);
            }
            (bool result, string? message) = await _repositoryLandTransaction.AddRangeAsync(newLandTransactions);
            return (result, message);
        }
    }
}
