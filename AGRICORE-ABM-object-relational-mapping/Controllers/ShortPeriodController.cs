using AGRICORE_ABM_object_relational_mapping.Services;
using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShortPeriodController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IAgriculturalProductionExtendedRepository _repositoryAgriculturalProduction;
        private readonly ILivestockProductionExtendedRepository _repositoryLivestockProduction;
        private readonly IRepository<HolderFarmYearData> _repositoryHolderFarmYearData;
        private readonly IRepository<ClosingValFarmValue> _repositoryClosingValFarmValue;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IRepository<FarmYearSubsidy> _repositoryFarmYearSubsidy;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IRepository<LandTransaction> _repositoryLandTransaction;
        private readonly IRepository<AgroManagementDecision> _repositoryAgroManagementDecisions;
        private readonly IRepository<GreeningFarmYearData> _repositoryGreeningFarmYear;
        private readonly IRepository<LandRent> _repositoryLandRent;
        private readonly IRepository<LogMessage> _repositoryLogMessage;
        private readonly IJsonObjService _jsonObjService;
        private readonly ILogger<ShortPeriodController> _logger;
        private readonly IMapper _mapper;

        public ShortPeriodController(
            IRepository<Population> repositoryPopulation,
            IRepository<Year> repositoryYear,
            IRepository<Farm> repositoryFarm,
            IAgriculturalProductionExtendedRepository repositoryAgriculturalProduction,
            ILivestockProductionExtendedRepository repositoryLiveStockProduction,
            IRepository<HolderFarmYearData> repositoryHolderFarmYearData,
            IRepository<Policy> repositoryPolicy,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
            IRepository<ClosingValFarmValue> repositoryClosingFarmValue,
            IRepository<FarmYearSubsidy> repositoryFarmYearSubsidy,
            IRepository<ProductGroup> repositoryProductGroup,
            IRepository<LandTransaction> repositoryLandTransaction,
            IRepository<AgroManagementDecision> repositoryAgroManagementDecisions,
            IRepository<GreeningFarmYearData> repositoryGreeningFarmYear,
            IRepository<LandRent> repositoryLandRent,
            IRepository<LogMessage> repositoryLogMessage,
            IJsonObjService jsonObjService,
            ILogger<ShortPeriodController> logger,
            IMapper mapper
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryYear = repositoryYear;
            _repositoryAgriculturalProduction = repositoryAgriculturalProduction;
            _repositoryClosingValFarmValue = repositoryClosingFarmValue;
            _repositoryPolicy = repositoryPolicy;
            _repositoryFarm = repositoryFarm;
            _repositoryHolderFarmYearData = repositoryHolderFarmYearData;
            _repositoryLivestockProduction = repositoryLiveStockProduction;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _repositoryFarmYearSubsidy = repositoryFarmYearSubsidy;
            _repositoryProductGroup = repositoryProductGroup;
            _repositoryLandTransaction = repositoryLandTransaction;
            _repositoryAgroManagementDecisions = repositoryAgroManagementDecisions;
            _repositoryGreeningFarmYear = repositoryGreeningFarmYear;
            _repositoryLandRent = repositoryLandRent;
            _jsonObjService = jsonObjService;
            _repositoryLogMessage = repositoryLogMessage;
            _logger = logger;
            _mapper = mapper;
        }

        public class GetDataForSPException : Exception
        {
            public GetDataForSPException()
            {
            }

            public GetDataForSPException(string message)
                : base(message)
            {
            }

            public GetDataForSPException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        /// <summary>
        /// Retrieves common simulation and calibration data for a given population and year.
        /// </summary>
        /// <param name="populationId">ID of the population to retrieve data for.</param>
        /// <param name="year">Year for which the data is being retrieved.</param>
        /// <param name="calibrationData">Indicates if the data is for calibration purposes.</param>
        /// <returns>DTO containing the common simulation and calibration data.</returns>

        private async Task<DataToSPDTO> GetCommonSimulationAndCalibrationData(long populationId, int year, bool calibrationData = true)
        {
            // For calibrating so we can simulate a given year, we need to get the data from the previous year
            var yearToQuery = year - 1;
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
            .Include(p => p.Years.Where(q => q.YearNumber == yearToQuery))
            .Include(p => p.Farms).ThenInclude(f => f.ClosingValFarmValues.Where(q => q.Year.YearNumber == yearToQuery))
            .Include(p => p.Farms).ThenInclude(f => f.AgriculturalProductions.Where(q => q.Year.YearNumber == yearToQuery))
            .Include(p => p.Farms).ThenInclude(f => f.LivestockProductions.Where(q => q.Year.YearNumber == yearToQuery)).ThenInclude(lp => lp.ProductGroup)
            .Include(p => p.Farms).ThenInclude(f => f.HoldersFarmYearData.Where(q => q.Year.YearNumber == yearToQuery))
            .Include(p => p.Farms).ThenInclude(f => f.GreeningFarmYearData.Where(q => q.Year.YearNumber == yearToQuery))
            .Include(p => p.Farms).ThenInclude(f => f.LandInRents.Where(q => q.Year.YearNumber == yearToQuery))
            .Include(p => p.Policies)
            .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.ProductGroup)
            .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.Policy)
            .Include(p => p.ProductGroups).ThenInclude(pg => pg.PolicyGroupRelations).ThenInclude(pgr => pgr.Policy),
                asNoTracking: true,
                asSeparateQuery: true
                );
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = $@"This population: {populationId} does not exist";
                _logger.LogError(error);
                throw new GetDataForSPException(error);
            }

            var thisYear = existingPopulation.Years?.Find(y => y.YearNumber == yearToQuery);

            if (thisYear == null)
            {
                error = $@"This population: {populationId} does not contain this year {yearToQuery}";
                _logger.LogError(error);
                throw new GetDataForSPException(error);
            }

            if (existingPopulation.Farms == null || existingPopulation.Farms.Count == 0)
            {
                error = "There are no farms for this population";
                _logger.LogError(error);
                throw new GetDataForSPException(error);
            }

            List<Farm> farmsData = existingPopulation.Farms;
            List<ProductGroup> productGroupData = existingPopulation.ProductGroups ?? new List<ProductGroup>();

            // We remove the other grouping as we agreed not to pass it to SP
            productGroupData.RemoveAll(pg => pg.IsOther());

            List<PolicyJsonDTO> policiesJson  = new List<PolicyJsonDTO>();
            List<PolicyGroupRelationJsonDTO> policyGroupRelationsJson  = new List<PolicyGroupRelationJsonDTO>();
            List<FarmYearSubsidyDTO> farmYearSubsidyDTOsJson  = new List<FarmYearSubsidyDTO>();

            // Calibration and simulation data deviate here as the policies available for the calibration were the ones active the previous year
            // and for simulation is the current
            var yearToUseForPoliciesAndSubsidies = calibrationData ? yearToQuery : year;

            policiesJson = _mapper.Map<List<PolicyJsonDTO>>(existingPopulation.Policies.Where(q => q.StartYearNumber <= yearToUseForPoliciesAndSubsidies && q.EndYearNumber >= yearToUseForPoliciesAndSubsidies).ToList());
            policyGroupRelationsJson = _mapper.Map<List<PolicyGroupRelationJsonDTO>>(existingPopulation.PolicyGroupRelations.Where(q => q.Policy.StartYearNumber <= yearToUseForPoliciesAndSubsidies && q.Policy.EndYearNumber >= yearToUseForPoliciesAndSubsidies));
            List<FarmYearSubsidy> farmYearSubsidies = await _repositoryFarmYearSubsidy.GetAllAsync(q => q.Policy.StartYearNumber <= yearToUseForPoliciesAndSubsidies && q.Policy.EndYearNumber >= yearToUseForPoliciesAndSubsidies && q.Policy.PopulationId == populationId, include: f=> f.Include(fys => fys.Policy).Include(fys => fys.Year), asNoTracking: true);
            foreach (var farmYearSubsidy in farmYearSubsidies)
            {
                var farmYearSubsidyDTO = _mapper.Map<FarmYearSubsidyDTO>(farmYearSubsidy);
                farmYearSubsidyDTO.YearNumber = yearToQuery;
                farmYearSubsidyDTOsJson.Add(farmYearSubsidyDTO);
            }

            List<ValueToSPDTO> values = new List<ValueToSPDTO>();

            foreach (var farm in farmsData)
            {
                var _crops = productGroupData.ToDictionary(pg => pg.Name, pg =>
                {
                    var agriProd = farm?.AgriculturalProductions?.FirstOrDefault(ap => ap.ProductGroupId == pg.Id);

                    //crop values from year-1
                    return new CropDataDTO
                    {
                        UAA = agriProd?.CultivatedArea ?? 0,
                        QuantitySold = agriProd?.QuantitySold ?? 0,
                        QuantityUsed = agriProd?.QuantityUsed ?? 0,
                        CropSellingPrice = agriProd?.SellingPrice ?? 0,
                        CoupledSubsidy = 0,
                        CropVariableCosts = agriProd?.VariableCosts ?? 0,
                        CropProductiveArea = (float)Math.Round(agriProd?.CultivatedArea ?? 0, 2),
                        RebreedingCows = 0,
                        DairyCows = 0
                    };
                });

                var livestockProduction = farm.LivestockProductions.FirstOrDefault(q => q.ProductGroup.Name == "DAIRY");
                HolderFarmYearData holderFarmYearData = farm.HoldersFarmYearData.SingleOrDefault();

                var rentInOperations = farm.LandInRents.Where(q => q.DestinationFarmId == farm.Id).ToList();

                values.Add(new ValueToSPDTO
                {
                    FarmCode = farm.Id, //create a class for this schema
                    HolderInfo = new HolderInfoDTO
                    {
                        HolderAge = holderFarmYearData.HolderAge,
                        HolderSuccessors = holderFarmYearData.HolderSuccessors,
                        HolderSuccessorsAge = holderFarmYearData.HolderSuccessorsAge,
                        HolderFamilyMembers = holderFarmYearData.HolderFamilyMembers,
                        HolderGender = holderFarmYearData.HolderGender.ToString()
                    },
                    Cod_RAGR = farm.RegionLevel1,
                    Cod_RAGR2 = farm.RegionLevel2,
                    Cod_RAGR3 = farm.RegionLevel3,
                    TechnicalEconomicOrientation = farm.TechnicalEconomicOrientation,
                    Altitude = farm.Altitude,
                    // This will be overwritten in the get data for simulation method with the data of the linked agromanagementdecission
                    CurrentAssets = farm.ClosingValFarmValues.SingleOrDefault().TotalCurrentAssets,
                    Crops = _crops,
                    Livestock = new LivestockDTO
                    {
                        NumberOfAnimals = livestockProduction?.NumberOfAnimals ?? 0,
                        DairyCows = livestockProduction?.DairyCows ?? 0,
                        RebreedingCows = livestockProduction?.NumberAnimalsRearingBreading ?? 0,
                        MilkProduction = (livestockProduction?.MilkProductionSold) ?? 0,
                        MilkSellingPrice = (livestockProduction?.MilkTotalSales / livestockProduction?.MilkProductionSold) ?? 0,
                        VariableCosts = livestockProduction?.MilkVariableCosts ?? 0,
                    },
                    GreeningSurface = farm.GreeningFarmYearData?.SingleOrDefault()?.GreeningSurface ?? 0,
                    RentedInLands = rentInOperations.Select(q => new LandRentDTO
                    {
                        OriginFarmId = q.OriginFarmId,
                        DestinationFarmId = q.DestinationFarmId,
                        YearId = q.YearId,
                        RentValue = q.RentValue,
                        RentArea = q.RentArea
                    }).ToList()
                });
            }

            DataToSPDTO result = new DataToSPDTO
            {
                Values = values,
                ProductGroups = _mapper.Map<List<ProductGroupJsonDTO>>(productGroupData),
                Policies = policiesJson,
                PolicyGroupRelations = policyGroupRelationsJson,
                FarmYearSubsidies = farmYearSubsidyDTOsJson
            };
            return result;
        }

        [HttpGet("/population/{populationId}/farms/get/calibrationdata/shortperiod")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataToSPDTO>> GetDataForShortPeriodCalibration(long populationId, [FromQuery] int year)
        {
            DataToSPDTO result = null;
            try
            {
                result = await GetCommonSimulationAndCalibrationData(populationId, year, calibrationData: true);
            }
            catch (GetDataForSPException e)
            {
                _logger.LogError("Error while retrieving data: " + e.Message);
                return StatusCode(409, e.Message);
            }
            
            if (result != null && result.Values.Any())
            {
                return Ok(result);
            }
            else
            {
                string error = "No data found for the given year.";
                _logger.LogError(error);
                return BadRequest(error);
            }

        }

        /// <summary>
        /// Retrieves data for short period simulation for a given population and year.
        /// </summary>
        /// <param name="populationId">ID of the population to retrieve data for.</param>
        /// <param name="year">Year for which the data is being retrieved.</param>
        /// <returns>DTO containing the data for short period simulation.</returns>

        [HttpGet("/population/{populationId}/farms/get/simulationdata/shortperiod")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DataToSPDTO>> GetDataForShortPeriodSimulation(long populationId, [FromQuery] int year)
        {
            DataToSPDTO result = null;
            try
            {
                result = await GetCommonSimulationAndCalibrationData(populationId, year, calibrationData: false);
                var farmIds = result.Values.Select(v => v.FarmCode).ToList();
                // Let's update the result accounting for land transfers and agromanagement decisions
                var landTransactions = await _repositoryLandTransaction.GetAllAsync(
                    predicate: t => t.Year.YearNumber == year && farmIds.Contains(t.DestinationFarmId), 
                    include: q => q.Include(q => q.Production).ThenInclude(q => q.Farm), asNoTracking: true);
                var agroManagementDecisions = await _repositoryAgroManagementDecisions.GetAllAsync(predicate: t => t.Year.YearNumber == year && farmIds.Contains(t.FarmId), include: q => q.Include(q => q.Farm), asNoTracking: true);
                foreach (var landTransaction in landTransactions)
                {
                    var productGroup = landTransaction.Production.ProductGroup.Name;

                    var originProduction = result.Values.SingleOrDefault(v => v.FarmCode == landTransaction.Production.Farm.Id).Crops[productGroup];
                    var destinationProduction = result.Values.SingleOrDefault(v => v.FarmCode == landTransaction.DestinationFarmId).Crops[productGroup];
                    bool notAlreadyExistingInDestination = false;
                    if (destinationProduction == null)
                    {
                        notAlreadyExistingInDestination = true;
                        destinationProduction = new CropDataDTO
                        {
                            CropProductiveArea = 0,
                            QuantityUsed = 0,
                            QuantitySold = 0,
                            CropSellingPrice = 0,
                            CropVariableCosts = 0,
                            CoupledSubsidy = 0,
                            UAA = 0,
                            RebreedingCows = 0,
                            DairyCows = 0
                        };
                    }
                    var transferedValues = new CropDataDTO
                    {
                        CropProductiveArea = (float)(originProduction.CropProductiveArea * landTransaction.Percentage),
                        QuantityUsed = originProduction.QuantityUsed * landTransaction.Percentage,
                        QuantitySold = originProduction.QuantitySold * landTransaction.Percentage,
                        CropSellingPrice = originProduction.CropSellingPrice,
                        CropVariableCosts = originProduction.CropVariableCosts,
                        CoupledSubsidy = originProduction.CoupledSubsidy * landTransaction.Percentage,
                        UAA = originProduction.UAA * landTransaction.Percentage,
                        // We don't transfer cows even when the land is transferred
                        RebreedingCows = 0,
                        DairyCows = 0
                    };
                    if (landTransaction.Percentage == 1)
                    {
                        originProduction.QuantitySold = 0;
                        originProduction.QuantityUsed = 0;
                        originProduction.CropProductiveArea = 0;
                        originProduction.CropSellingPrice = 0;
                        originProduction.CropVariableCosts = 0;
                        originProduction.CoupledSubsidy = 0;
                        originProduction.UAA = 0;
                        // We don't transfer cows even when the land is transferred
                        originProduction.RebreedingCows = originProduction.RebreedingCows ?? 0;
                        originProduction.DairyCows = originProduction.DairyCows ?? 0;
                    } else
                    {
                        originProduction.QuantitySold -= transferedValues.QuantitySold;
                        originProduction.QuantityUsed -= transferedValues.QuantityUsed;
                        originProduction.CropProductiveArea -= transferedValues.CropProductiveArea;
                        originProduction.CropSellingPrice = originProduction.CropSellingPrice; // This should not change
                        originProduction.CropVariableCosts = originProduction.CropVariableCosts;// This should not change
                        originProduction.CoupledSubsidy -= transferedValues.CoupledSubsidy;
                        originProduction.UAA -= transferedValues.UAA;
                        // We don't transfer cows even when the land is transferred
                        originProduction.RebreedingCows = originProduction.RebreedingCows ?? 0;
                        originProduction.DairyCows = originProduction.DairyCows ?? 0;
                    }
                    destinationProduction.QuantitySold += transferedValues.QuantitySold;
                    destinationProduction.QuantityUsed += transferedValues.QuantityUsed;
                    destinationProduction.CropProductiveArea += transferedValues.CropProductiveArea;
                    if (notAlreadyExistingInDestination)
                    {
                        destinationProduction.CropVariableCosts += transferedValues.CropVariableCosts;
                    } else
                    {
                        destinationProduction.CropVariableCosts += transferedValues.CropVariableCosts; //we assume that the new land will use the same technology level as the already owned
                    }
                    destinationProduction.CoupledSubsidy += transferedValues.CoupledSubsidy;
                    destinationProduction.UAA += transferedValues.UAA;
                    destinationProduction.CropSellingPrice = 
                        (transferedValues.CropSellingPrice * transferedValues.UAA + destinationProduction.CropSellingPrice * destinationProduction.UAA)
                        / (transferedValues.UAA + destinationProduction.UAA);
                    destinationProduction.RebreedingCows = destinationProduction.RebreedingCows ?? 0;
                    destinationProduction.DairyCows = destinationProduction.DairyCows ?? 0;
                }
                foreach (var agroManagementDecision in agroManagementDecisions)
                {
                    var affectedFarm = result.Values.SingleOrDefault(v => v.FarmCode == agroManagementDecision.FarmId);
                    affectedFarm.CurrentAssets = agroManagementDecision.TotalCurrentAssets;
                }
            }
            catch (GetDataForSPException e)
            {
                _logger.LogError("Error while retrieving data: " + e.Message);
                return StatusCode(409, e.Message);
            }

            if (result != null && result.Values.Any())
            {
                return Ok(result);
            }
            else
            {
                string error = "No data found for the given year.";
                _logger.LogError(error);
                return BadRequest(error);
            }
        }

        /// <summary>
        /// Adds the data coming from the short period simulation for a given year.
        /// </summary>
        /// <param name="spSimulationValues">Values returned by the short period simulation.</param>
        /// <param name="year">Year for which the data is being added.</param>
        /// <param name="simulationRunId">Id of the simulation run.</param>
        /// <returns>Ok if the data was correctly added and error otherwise.</returns>

        [HttpPut("/results/shortperiod/simulation")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [DisableRequestSizeLimit]

        public async Task<ActionResult> AddValuesFromSP(List<ValueFromSPDTO> spSimulationValues, [FromQuery] int year, long simulationRunId = 0)
        {
            try
            {
                string error = string.Empty;
                if (spSimulationValues.Count() == 0)
                {
                    error = $@"The provided data do not contain any result";
                    _logger.LogError(error);
                    return StatusCode(409, error);
                }
                // We suppose that all farms are from the same population
                var inputFarmIds = spSimulationValues.Select(q => q.FarmId).Distinct().ToList();
                var populationId = (await _repositoryFarm.GetSingleOrDefaultAsync(predicate: f => f.Id == inputFarmIds[0], asNoTracking: true))?.PopulationId ?? 0;
                if (populationId == 0)
                {
                    error = $@"First farm of the results is not related to any population in the DB";
                    _logger.LogError(error);
                    return StatusCode(409, error);
                }
                
                var populationYears = await _repositoryYear.GetAllAsync(predicate: y => y.PopulationId == populationId, asNoTracking: true);
                var targetYear = populationYears.SingleOrDefault(y => y.YearNumber == year);
                if (targetYear == null)
                {
                    error = $@"This population: {populationId} does not contain the year {year}";
                    _logger.LogError(error);
                    return StatusCode(409, error);
                }
                var previousYear = populationYears.SingleOrDefault(y => y.YearNumber == year - 1);
                if (previousYear == null)
                {
                    error = $@"This population: {populationId} does not contain the year {year - 1}";
                    _logger.LogError(error);
                    return StatusCode(409, error);
                }

                await _repositoryLogMessage.AddAsync(new LogMessage
                {
                    SimulationRunId = simulationRunId,
                    Description = $"Adding ST simulation results for year {year} for population {populationId}",
                    LogLevel = DB.Data.Models.LogLevel.INFO,
                    TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Source = "ShortPeriodController - ABM",
                    Title = "ST Simulation Results being processed"
                });

                // Cleaning data in case this is a re-run
                var previousRunClosingValues = await _repositoryClosingValFarmValue.GetAllAsync(predicate: t => t.YearId == targetYear.Id && t.Farm.PopulationId == populationId);
                if (previousRunClosingValues.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryClosingValFarmValue.RemoveRange(previousRunClosingValues);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous closing values: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var previousRunAgriculturalProductions = await _repositoryAgriculturalProduction.GetAllAsync(x => x.YearId == targetYear.Id && x.Farm.PopulationId == populationId);
                if (previousRunAgriculturalProductions.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryAgriculturalProduction.RemoveRange(previousRunAgriculturalProductions);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous agricultural productions: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var previousRunLivestockProductions = await _repositoryLivestockProduction.GetAllAsync(x => x.YearId == targetYear.Id && x.Farm.PopulationId == populationId);
                if (previousRunLivestockProductions.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryLivestockProduction.RemoveRange(previousRunLivestockProductions);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous livestock productions: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var previousRunFarmSubsidies = await _repositoryFarmYearSubsidy.GetAllAsync(x => x.YearId == targetYear.Id && x.Farm.PopulationId == populationId);
                if (previousRunFarmSubsidies.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryFarmYearSubsidy.RemoveRange(previousRunFarmSubsidies);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous farm subsidies: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var previousRunGreeningAllocations = await _repositoryGreeningFarmYear.GetAllAsync(x => x.YearId == targetYear.Id && x.Farm.PopulationId == populationId);
                if (previousRunGreeningAllocations.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryGreeningFarmYear.RemoveRange(previousRunGreeningAllocations);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous greening allocations: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var previousLandRents = await _repositoryLandRent.GetAllAsync(x => x.YearId == targetYear.Id && x.OriginFarm.PopulationId == populationId);
                if (previousLandRents.Any())
                {
                    var (deleteSuccess, deleteMessage) = _repositoryLandRent.RemoveRange(previousLandRents);
                    if (!deleteSuccess)
                    {
                        error = $@"Error while deleting previous LandRents allocations: {deleteMessage}";
                        _logger.LogError(error);
                        return StatusCode(409, error);
                    }
                }

                var productsList = await _repositoryProductGroup.GetAllAsync(predicate: t => t.PopulationId == populationId, include: t => t.Include(q => q.PolicyGroupRelations).ThenInclude(q => q.Policy), asSeparateQuery: true, asNoTracking: true);
                var otherProductGroupsIds = productsList.Where(p => p.IsOther()).Select(q => q.Id).ToList();
                var policies = await _repositoryPolicy.GetAllAsync(predicate: p => p.PopulationId == populationId, include: p => p.Include(q => q.PolicyGroupRelations).ThenInclude(q => q.ProductGroup), asNoTracking: true, asSeparateQuery: true);
                // Ids of policies for green pension and basic
                var otherPolicyIds = (await _repositoryPolicy.GetAllAsync(
                    predicate: p => p.PopulationId == populationId && p.PolicyGroupRelations != null && p.PolicyGroupRelations.Any(q => otherProductGroupsIds.Contains(q.ProductGroupId)),
                    include: p => p.Include(q => q.PolicyGroupRelations), asNoTracking: true)).
                    Select(p => p.Id).ToList();

                // Let's do this in a loop per each one of the regions
                var populationFarms = await _repositoryFarm.GetAllAsync(predicate: f => f.PopulationId == populationId, asNoTracking: true);
                var regions = populationFarms.Select(f => f.RegionLevel3).Distinct().ToList();

                // Get data to reuse between regions
                var closingValues = await _repositoryClosingValFarmValue.GetAllAsync(predicate: t => t.YearId == previousYear.Id && t.Farm.PopulationId == populationId, asNoTracking: true);

                // Vars to calculate effectivity of the crop and livestock productions regarding costs
                var averageAgriculturalProductionCosts = await _repositoryAgriculturalProduction.GetRegionDictionaryOfVariableCosts(populationId);
                var averageLivestockProductionCosts = await _repositoryLivestockProduction.GetDictionaryOfVariableCosts(populationId);
                var averageMilkProductionCosts = await _repositoryLivestockProduction.GetDictionaryOfMilkVariableCosts(populationId);

                int count = 0;
                foreach (var region in regions) {
                    count++;
                    await _repositoryLogMessage.AddAsync(new LogMessage
                    {
                        SimulationRunId = simulationRunId,
                        Description = $"-->ST simulation results record. Processing region {count} of {regions.Count} for year {year} for population {populationId}",
                        LogLevel = DB.Data.Models.LogLevel.INFO,
                        TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        Source = "ShortPeriodController - ABM",
                        Title = $"ST Simulation Results being processed. Region {count} of {regions.Count}"
                    });

                    var regionFarms = await _repositoryFarm.GetAllAsync(
                        predicate: f => f.PopulationId == populationId && f.RegionLevel3 == region,
                        include: f => f.Include(q => q.ClosingValFarmValues).Include(q => q.HoldersFarmYearData).Include(q => q.GreeningFarmYearData).Include(q => q.LandInRents), asNoTracking: true, asSeparateQuery: true);
                    var affectedFarms = regionFarms.Where(f => inputFarmIds.Contains(f.Id));
                    var affectedFarmsIds = affectedFarms.Select(f => f.Id).ToList();
                    var notAffectedFarms = regionFarms.Where(f => !inputFarmIds.Contains(f.Id));
                    var notAffectedFarmsIds = notAffectedFarms.Select(f => f.Id).ToList();
                    var allFarmsIds = affectedFarmsIds.Concat(notAffectedFarmsIds).ToList();

                    var previousYearSubsidies = await _repositoryFarmYearSubsidy.GetAllAsync(predicate: t => t.YearId == previousYear.Id && t.Farm.RegionLevel3 == region, include: q => q.Include(q => q.Policy).Include(q => q.Farm), asNoTracking: true, asSeparateQuery: true);
                    var previousYearSubsidiesNotAffected = previousYearSubsidies.Where(q => notAffectedFarmsIds.Contains(q.FarmId)).ToList();

                    // Transactions are ignored here as the data is already included in the agromanagementDecision economic values and in the farms arrangements from the SP.
                    //var transactionList = await _repositoryLandTransaction.GetAllAsync(predicate: t => t.YearId == targetYear.Id && inputFarmIds.Contains(t.Production.Farm.Id), include: q => q.Include(q => q.Production).ThenInclude(q => q.Farm));
                    var agroManagementDecisionList = await _repositoryAgroManagementDecisions.GetAllAsync(
                        predicate: t => t.YearId == targetYear.Id && inputFarmIds.Contains(t.FarmId),
                        include: q => q.Include(q => q.Farm)
                        , asNoTracking: true, asSeparateQuery: true);

                    // Indeed we need the transfers to update other sizes and land value
                    var regionLandTransactions = await _repositoryLandTransaction.GetAllAsync(
                        predicate: t => t.YearId == targetYear.Id && t.DestinationFarm.RegionLevel3 == region,
                        include:
                            q => q.Include(q => q.Production).ThenInclude(q => q.Farm).Include(q => q.DestinationFarm).
                                 Include(q => q.Production).ThenInclude(q => q.ProductGroup), asNoTracking: true, asSeparateQuery: true);

                    

                    // Calculations
                    var toConsiderClosingValuesForRates = closingValues.Where(q => q.GrossFarmIncome != 0).ToList();
                    var averageTaxGrossIncomeRate = toConsiderClosingValuesForRates != null && toConsiderClosingValuesForRates.Count > 0 ? toConsiderClosingValuesForRates.Average(q => q.Taxes / q.GrossFarmIncome) : 0;
                    var averageVatGrossIncomeRate = toConsiderClosingValuesForRates != null && toConsiderClosingValuesForRates.Count > 0 ? toConsiderClosingValuesForRates.Average(q => q.VatBalanceExcludingInvestments / q.GrossFarmIncome) : 0;

                    // Vars to calculate effectivity of the crop and livestock productions regarding costs
                    var previousYearAgriculturalProductions = await _repositoryAgriculturalProduction.GetAllAsync(
                        predicate: ap => ap.YearId == previousYear.Id && ap.Farm.RegionLevel3 == region,
                        include: f => f.Include(f => f.Farm),
                        asNoTracking: true);
                    var previousYearLivestockProductions = await _repositoryLivestockProduction.GetAllAsync(
                        predicate: ap => ap.YearId == previousYear.Id && ap.Farm.RegionLevel3 == region,
                        include: f => f.Include(f => f.Farm), 
                        asNoTracking: true);

                    List<AgriculturalProduction> newAgriculturalProductions = new List<AgriculturalProduction>();
                    List<LivestockProduction> newLivestockProductions = new List<LivestockProduction>();
                    List<ClosingValFarmValue> newClosingValFarmValues = new List<ClosingValFarmValue>();
                    List<FarmYearSubsidy> newFarmYearSubsidies = new List<FarmYearSubsidy>();
                    List<GreeningFarmYearData> newGreeningAllocations = new List<GreeningFarmYearData>();
                    List<LandRent> newLandRents = new List<LandRent>();
                    

                    //We keep the land rents in which both farms (origin, destination) are not included in the simulation value.
                    var previousRentsToKeep = affectedFarms.SelectMany(f => f.LandInRents).Where(l => !inputFarmIds.Contains(l.OriginFarmId) && !inputFarmIds.Contains(l.DestinationFarmId)).ToList();
                    foreach (var rent in previousRentsToKeep)
                    {
                        newLandRents.Add(new LandRent
                        {
                            OriginFarmId = rent.OriginFarmId,
                            DestinationFarmId = rent.DestinationFarmId,
                            RentArea = rent.RentArea,
                            RentValue = rent.RentValue,
                            YearId = targetYear.Id
                        });
                    }

                    foreach (var farm in regionFarms)
                    {
                        bool simulatedFarm = affectedFarmsIds.Contains(farm.Id);

                        var previousYearClosingValue = farm.ClosingValFarmValues.SingleOrDefault(c => c.YearId == previousYear.Id);
                        if (previousYearClosingValue == null)
                        {
                            error = $@"This farm {farm.Id} does not have a closing value for the previous year: {previousYear.YearNumber}";
                            _logger.LogError(error);
                            return StatusCode(409, error);
                        }

                        var thisFarmPreviousYearSubsidies = previousYearSubsidies.Where(q => q.FarmId == farm.Id).ToList();

                        var thisFarmPreviousYearCropProductions = previousYearAgriculturalProductions.Where(q => q.FarmId == farm.Id).ToList();
                        var thisFarmPreviousYearLivestockProductions = previousYearLivestockProductions.Where(q => q.FarmId == farm.Id).ToList();

                        var simulationValue = spSimulationValues.SingleOrDefault(s => s.FarmId == farm.Id);
                        // fix for minor renting balances
                        if (simulationValue != null && Math.Abs(simulationValue.RentBalanceArea) < 0.1)
                        {
                            simulationValue.RentBalanceArea = 0;
                        }
                        var agromanagementDecision = agroManagementDecisionList.SingleOrDefault(a => a.FarmId == farm.Id && a.YearId == targetYear.Id);
                        // We create the new closingValFarmValues object for the farm in the target year
                        var thisYearClosingValue = new ClosingValFarmValue
                        {
                            FarmId = farm.Id,
                            YearId = targetYear.Id,
                            // We set the values that are provided by the SP. If not present, we use previous year values
                            TotalCurrentAssets = simulationValue?.TotalCurrentAssets ?? previousYearClosingValue.TotalCurrentAssets,
                            FarmNetIncome = 0,
                            GrossFarmIncome = 0,
                            // The price of land is fixed right now in 589€/year/ha. However, we need a new entity to store this value
                            RentBalance = simulationValue?.RentBalanceArea * (-589) ?? 0,
                            // These values are inherited from the previous year as we don't deal with them
                            FarmBuildingsValue = previousYearClosingValue.FarmBuildingsValue,
                            FixedAssets = previousYearClosingValue.FixedAssets,
                            ForestLandArea = previousYearClosingValue.ForestLandArea,
                            ForestLandValue = previousYearClosingValue.ForestLandValue,
                            IntangibleAssetsNonTradable = previousYearClosingValue.IntangibleAssetsNonTradable,
                            IntangibleAssetsTradable = previousYearClosingValue.IntangibleAssetsTradable,
                            LandImprovements = previousYearClosingValue.LandImprovements,
                            Machinery = previousYearClosingValue.Machinery,
                            MachineryAndEquipment = previousYearClosingValue.MachineryAndEquipment,
                            OtherNonCurrentAssets = previousYearClosingValue.OtherNonCurrentAssets,
                            OtherOutputs = previousYearClosingValue.OtherOutputs,
                            SubsidiesOnInvestments = previousYearClosingValue.SubsidiesOnInvestments,
                            TotalExternalFactors = previousYearClosingValue.TotalExternalFactors,
                            TotalIntermediateConsumption = previousYearClosingValue.TotalIntermediateConsumption,
                            VATBalanceOnInvestments = previousYearClosingValue.VATBalanceOnInvestments,
                            PlantationsValue = previousYearClosingValue.PlantationsValue,
                            // These values should be extracted from the Agromanagement Decisions
                            LongAndMediumTermLoans = agromanagementDecision?.LongAndMediumTermLoans ?? previousYearClosingValue.LongAndMediumTermLoans,
                            AgriculturalLandValue = agromanagementDecision?.AgriculturalLandValue ?? previousYearClosingValue.AgriculturalLandValue,
                            AgriculturalLandArea = agromanagementDecision?.AgriculturalLandArea ?? previousYearClosingValue.AgriculturalLandArea,
                            // These values should be properly updated in a yearly basis. Review Equations
                            Depreciation = previousYearClosingValue.Depreciation,
                            Taxes = 0,
                            TotalOutputCropsAndCropProduction = 0, //updated after adding the crops
                            TotalOutputLivestockAndLivestockProduction = 0, //updated after adding the livestockproductions
                            VatBalanceExcludingInvestments = 0
                        };

                        var thisFarmNewLivestockProductions = new List<LivestockProduction>();
                        var thisFarmNewAgriculturalProductions = new List<AgriculturalProduction>();
                        var thisFarmNewFarmYearSubsidies = new List<FarmYearSubsidy>();
                        // We get the greening allocations for the farm excluding any of previous runs for the same year
                        var thisFarmNewGreeningAllocations = new List<GreeningFarmYearData>();

                        if (simulationValue != null)
                        {
                            var totalVariableCosts = simulationValue.TotalVariableCosts;
                            foreach (var rent in simulationValue.RentedInLands)
                            {
                                newLandRents.Add(new LandRent
                                {
                                    OriginFarmId = rent.OriginFarmId,
                                    DestinationFarmId = rent.DestinationFarmId,
                                    RentArea = rent.RentArea,
                                    RentValue = rent.RentValue,
                                    YearId = targetYear.Id
                                });
                            }
                            foreach (var cropKeyValue in simulationValue.Crops)
                            {
                                var cropName = cropKeyValue.Key;
                                var cropData = cropKeyValue.Value;

                                // Perform a different action for "MILK" crop -> livestock
                                if (string.Equals(cropName, "MILK", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var thisProductGroupId = productsList
                                        .Where(p => p.Name == "DAIRY")
                                        .Select(p => p.Id)
                                        .FirstOrDefault();
                                    var newLivestockProduction = new LivestockProduction
                                    {
                                        FarmId = farm.Id,
                                        YearId = targetYear.Id,
                                        ProductGroupId = thisProductGroupId,
                                        NumberOfAnimals = 0, // To be defined in SP
                                        DairyCows = (cropData.DairyCows != null ? (int)(Math.Round(cropData.DairyCows.Value, 0)) : 0),
                                        VariableCosts = cropData.CropVariableCosts,
                                        NumberAnimalsRearingBreading = (cropData.RebreedingCows != null ? (int)(Math.Round(cropData.RebreedingCows.Value, 0)) : 0),
                                        MilkProductionSold = cropData.QuantitySold,
                                        MilkTotalProduction = 0,
                                        MilkTotalSales = cropData.QuantitySold * cropData.CropSellingPrice ?? 0,
                                        SellingPrice = cropData.CropSellingPrice,
                                        MilkVariableCosts = cropData.CropVariableCosts,
                                        ValueSoldAnimals = 0,
                                        NumberAnimalsForSlaughtering = 0,
                                        ValueAnimalsRearingBreading = 0,
                                        ValueSlaughteredAnimals = 0
                                    };
                                    newLivestockProduction.NumberOfAnimals = newLivestockProduction.DairyCows + newLivestockProduction.NumberAnimalsRearingBreading;

                                    var thisFarmPreviousYearMilkProduction = thisFarmPreviousYearLivestockProductions.SingleOrDefault(q => q.ProductGroupId == thisProductGroupId);
                                    if (thisFarmPreviousYearMilkProduction != null)
                                    {
                                        //This is a temporal patch while the system includes proper derivate products management, to paliate for the not accounted "used" milk in such products
                                        if (thisFarmPreviousYearMilkProduction.MilkTotalSales > 0)
                                        {
                                            newLivestockProduction.MilkTotalProduction = newLivestockProduction.MilkTotalSales * thisFarmPreviousYearMilkProduction.MilkTotalProduction / thisFarmPreviousYearMilkProduction.MilkTotalSales;
                                        }
                                        //These patch allows to keep previous years information for other non-contemplated elements
                                        if (thisFarmPreviousYearMilkProduction.ValueSoldAnimals > 0)
                                        {
                                            newLivestockProduction.ValueSoldAnimals = thisFarmPreviousYearMilkProduction.ValueSoldAnimals;
                                        }
                                        if (thisFarmPreviousYearMilkProduction.ValueAnimalsRearingBreading > 0)
                                        {
                                            newLivestockProduction.ValueAnimalsRearingBreading = thisFarmPreviousYearMilkProduction.ValueAnimalsRearingBreading;
                                        }
                                        if (thisFarmPreviousYearMilkProduction.ValueSlaughteredAnimals > 0)
                                        {
                                            newLivestockProduction.ValueSlaughteredAnimals = thisFarmPreviousYearMilkProduction.ValueSlaughteredAnimals;
                                        }
                                        if (thisFarmPreviousYearMilkProduction.ValueAnimalsRearingBreading > 0)
                                        {
                                            newLivestockProduction.ValueAnimalsRearingBreading = thisFarmPreviousYearMilkProduction.ValueAnimalsRearingBreading;
                                        }
                                    }

                                    thisFarmNewLivestockProductions.Add(newLivestockProduction);
                                }
                                //else, if its an agri crop, we define the parameters to store also for year+1
                                else
                                {
                                    var thisProduct = productsList
                                        .Where(p => p.Name == cropName)
                                        .FirstOrDefault();
                                    var thisProductGroupId = thisProduct.Id;

                                    var thisFarmPreviousYearCropProduction = thisFarmPreviousYearCropProductions.SingleOrDefault(q => q.ProductGroupId == thisProductGroupId);

                                    var newAgriculturalProduction = new AgriculturalProduction
                                    {
                                        FarmId = farm.Id,
                                        YearId = targetYear.Id,
                                        ProductGroupId = thisProductGroupId,
                                        CultivatedArea = cropData.CropProductiveArea,
                                        // This will be 0 as the SP always return 0
                                        VariableCosts = cropData.CropVariableCosts,
                                        CropProduction = (cropData.QuantitySold + cropData.QuantityUsed) * cropData.CropSellingPrice,
                                        SellingPrice = cropData.CropSellingPrice,
                                        ValueSales = cropData.QuantitySold * cropData.CropSellingPrice,
                                        QuantitySold = cropData.QuantitySold,
                                        QuantityUsed = cropData.QuantityUsed,
                                        LandValue = (thisYearClosingValue.AgriculturalLandArea >= 0) ? cropData.CropProductiveArea * thisYearClosingValue.AgriculturalLandValue / thisYearClosingValue.AgriculturalLandArea : 0,
                                        IrrigatedArea = 0, // To be defined in SP
                                        OrganicProductionType = thisProduct.Organic, // To be defined in SP
                                    };

                                    // Fix irrigated area as it is not being provided by the SP. Assume same ratio
                                    if (thisFarmPreviousYearCropProduction != null && thisFarmPreviousYearCropProduction.IrrigatedArea > 0 && thisFarmPreviousYearCropProduction.CultivatedArea > 0)
                                    {
                                        newAgriculturalProduction.IrrigatedArea = thisFarmPreviousYearCropProduction.IrrigatedArea * newAgriculturalProduction.CultivatedArea / thisFarmPreviousYearCropProduction.CultivatedArea;
                                        
                                    }
                                    if (!thisFarmNewAgriculturalProductions.Any(q => q.ProductGroupId == newAgriculturalProduction.ProductGroupId && q.FarmId == newAgriculturalProduction.FarmId && q.YearId == newAgriculturalProduction.YearId))
                                    {
                                        thisFarmNewAgriculturalProductions.Add(newAgriculturalProduction);
                                    } else
                                    {
                                        _logger.LogWarning("Trying to add a duplicate Agricultural production, please check the data");
                                    }
                                }
                                foreach (var coupledSubsidy in simulationValue.Subsidies)
                                {
                                    var thisPolicy = policies.Single(q => q.PolicyIdentifier == coupledSubsidy.PolicyIdentifier);
                                    var thisPolicyGroupRelations = thisPolicy.PolicyGroupRelations;

                                    if (thisPolicyGroupRelations!= null && thisPolicyGroupRelations.Count() >0)
                                    {
                                        var totalCompensation = thisPolicyGroupRelations.Sum(q => q.EconomicCompensation);
                                        foreach (var thisPolicyRelation in thisPolicyGroupRelations)
                                        {
                                            var valueToRecord =
                                                    thisPolicyRelation.EconomicCompensation > 0 ?
                                                    coupledSubsidy.Value * thisPolicyRelation.EconomicCompensation / totalCompensation
                                                    : 0;
                                            if (thisFarmNewFarmYearSubsidies.Any(q => q.PolicyId == thisPolicy.Id))
                                            {
                                                var thisFarmYearSubsidy = thisFarmNewFarmYearSubsidies.Single(q => q.PolicyId == thisPolicy.Id);
                                                thisFarmYearSubsidy.Value += valueToRecord;
                                            }
                                            else
                                            {
                                                thisFarmNewFarmYearSubsidies.Add(new FarmYearSubsidy
                                                {
                                                    FarmId = farm.Id,
                                                    YearId = targetYear.Id,
                                                    PolicyId = thisPolicy.Id,
                                                    Value = valueToRecord
                                                }) ;
                                            }
                                        }
                                    }
                                }

                            }

                            // Now we need to fix the individual variable costs for the farms
                            // we assume that variable costs of existing productions in the previous year keep constant (this includes others)
                            // and the others are distributed among the new productions considering their distribution in the previous year
                            // and we count on the SP to having considered the "effectiveness" of the farm on this
                            float accumulatedVariableCosts = 0;
                            float toDistributeVariableCosts = 0;

                            List<long> toDistributeProductIds = new List<long>();
                            foreach (var cropProduction in thisFarmNewAgriculturalProductions)
                            {
                                var previousYearCropProduction = thisFarmPreviousYearCropProductions.SingleOrDefault(q => q.ProductGroupId == cropProduction.ProductGroupId);
                                if (previousYearCropProduction != null && previousYearCropProduction.VariableCosts != 0)
                                {
                                    cropProduction.VariableCosts = previousYearCropProduction.VariableCosts;
                                    accumulatedVariableCosts += previousYearCropProduction.VariableCosts ?? 0 * (cropProduction.QuantitySold + cropProduction.QuantityUsed) ?? 0;
                                }
                                else
                                {
                                    toDistributeVariableCosts += cropProduction.CropProduction ?? 0 * averageAgriculturalProductionCosts[cropProduction.ProductGroupId.Value];
                                    toDistributeProductIds.Add(cropProduction.ProductGroupId.Value);
                                }
                            }
                            foreach (var livestockProduction in thisFarmNewLivestockProductions)
                            {
                                var previousYearLivestockProduction = thisFarmPreviousYearLivestockProductions.SingleOrDefault(q => q.ProductGroupId == livestockProduction.ProductGroupId);
                                
                                // The whole whay derived products from livestock need to be rethinked to avoid so cumbersome calculations
                                
                                if (previousYearLivestockProduction != null && previousYearLivestockProduction.MilkVariableCosts != 0)
                                {
                                    livestockProduction.MilkVariableCosts = previousYearLivestockProduction.MilkVariableCosts;
                                    livestockProduction.VariableCosts = previousYearLivestockProduction.VariableCosts;
                                    accumulatedVariableCosts += previousYearLivestockProduction.MilkVariableCosts * livestockProduction.MilkTotalProduction;
                                }
                                else
                                {
                                    toDistributeVariableCosts += (livestockProduction.EggsTotalProduction + livestockProduction.MilkTotalProduction + livestockProduction.WoolTotalProduction) * averageLivestockProductionCosts[livestockProduction.ProductGroupId];
                                    toDistributeProductIds.Add(livestockProduction.ProductGroupId);
                                }
                            }
                            var unknownVariableCosts = totalVariableCosts - accumulatedVariableCosts;
                            var unknownVariableCostsRatio = unknownVariableCosts / toDistributeVariableCosts;

                            foreach (var productGroupId in toDistributeProductIds)
                            {
                                var newAgriculturalProduction = thisFarmNewAgriculturalProductions.SingleOrDefault(q => q.ProductGroupId == productGroupId);
                                if (newAgriculturalProduction != null)
                                {
                                    newAgriculturalProduction.VariableCosts = averageAgriculturalProductionCosts[productGroupId] * unknownVariableCostsRatio;
                                }
                                else
                                {
                                    var newLivestockProduction = thisFarmNewLivestockProductions.SingleOrDefault(q => q.ProductGroupId == productGroupId);
                                    if (newLivestockProduction != null)
                                    {
                                        newLivestockProduction.VariableCosts = (averageLivestockProductionCosts[productGroupId] + averageMilkProductionCosts[productGroupId]) * unknownVariableCostsRatio;
                                    }
                                }
                            }

                            // We translate from one year to the OTHER productions (categories include OTHER)
                            var thisFarmPreviousYearOtherCropProductions = thisFarmPreviousYearCropProductions.
                                Where(q => otherProductGroupsIds.Contains(q.ProductGroupId ?? 0)).ToList();

                            var thisFarmOtherOutTransactions = regionLandTransactions.Where(q => q.Production.FarmId == farm.Id).ToList();
                            var thisFarmOtherInTransactions = regionLandTransactions.Where(q => q.DestinationFarmId == farm.Id).ToList();

                            if (thisFarmOtherInTransactions.Count() == 0 && thisFarmOtherOutTransactions.Count() == 0)
                            {
                                // If the farms has no "OTHER" agricultural production, we skip the process
                                foreach (var previousYearOtherCropProduction in thisFarmPreviousYearOtherCropProductions)
                                {
                                    var newAgriculturalProduction = new AgriculturalProduction
                                    {
                                        FarmId = farm.Id,
                                        YearId = targetYear.Id,
                                        ProductGroupId = previousYearOtherCropProduction.ProductGroupId,
                                        CultivatedArea = previousYearOtherCropProduction.CultivatedArea,
                                        VariableCosts = previousYearOtherCropProduction.VariableCosts,
                                        CropProduction = previousYearOtherCropProduction.CropProduction,
                                        SellingPrice = previousYearOtherCropProduction.SellingPrice,
                                        IrrigatedArea = previousYearOtherCropProduction.IrrigatedArea,
                                        LandValue = previousYearOtherCropProduction.LandValue,
                                        OrganicProductionType = previousYearOtherCropProduction.OrganicProductionType,
                                        QuantitySold = previousYearOtherCropProduction.QuantitySold,
                                        QuantityUsed = previousYearOtherCropProduction.QuantityUsed,
                                        ValueSales = previousYearOtherCropProduction.ValueSales
                                    };
                                    if (!thisFarmNewAgriculturalProductions.Any(q => q.ProductGroupId == newAgriculturalProduction.ProductGroupId && q.FarmId == newAgriculturalProduction.FarmId && q.YearId == newAgriculturalProduction.YearId))
                                    {
                                        thisFarmNewAgriculturalProductions.Add(newAgriculturalProduction);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Trying to add a duplicate Agricultural production, please check the data");
                                    }
                                }
                            } else if (thisFarmOtherOutTransactions.Count() > 0)
                            {
                                foreach (var previousYearOtherCropProduction in thisFarmPreviousYearOtherCropProductions)
                                {
                                    var thisPreviousYearOtherCropProductionProductId = previousYearOtherCropProduction.ProductGroupId;
                                    var sameProductTransfer = thisFarmOtherOutTransactions.Where(q => q.Production.ProductGroupId == thisPreviousYearOtherCropProductionProductId).ToList();
                                    var totalPercentageSold = sameProductTransfer.Count > 0 ? sameProductTransfer.Sum(q => q.Percentage) : 0;

                                    if (totalPercentageSold < 1 && totalPercentageSold >= 0)
                                    {
                                        var newAgriculturalProduction = new AgriculturalProduction
                                        {
                                            FarmId = farm.Id,
                                            YearId = targetYear.Id,
                                            ProductGroupId = previousYearOtherCropProduction.ProductGroupId,
                                            CultivatedArea = previousYearOtherCropProduction.CultivatedArea * (1 - totalPercentageSold),
                                            VariableCosts = previousYearOtherCropProduction.VariableCosts,
                                            CropProduction = previousYearOtherCropProduction.CropProduction * (1 - totalPercentageSold),
                                            SellingPrice = previousYearOtherCropProduction.SellingPrice,
                                            IrrigatedArea = previousYearOtherCropProduction.IrrigatedArea * (1 - totalPercentageSold),
                                            LandValue = previousYearOtherCropProduction.LandValue * (1 - totalPercentageSold),
                                            OrganicProductionType = previousYearOtherCropProduction.OrganicProductionType,
                                            QuantitySold = previousYearOtherCropProduction.QuantitySold * (1 - totalPercentageSold),
                                            QuantityUsed = previousYearOtherCropProduction.QuantityUsed * (1 - totalPercentageSold),
                                            ValueSales = previousYearOtherCropProduction.ValueSales * (1 - totalPercentageSold),
                                        };
                                        if (!thisFarmNewAgriculturalProductions.Any(q => q.ProductGroupId == newAgriculturalProduction.ProductGroupId && q.FarmId == newAgriculturalProduction.FarmId && q.YearId == newAgriculturalProduction.YearId))
                                        {
                                            thisFarmNewAgriculturalProductions.Add(newAgriculturalProduction);
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Trying to add a duplicate Agricultural production, please check the data");
                                        }
                                    }
                                    // If its a full transfer (percentage 1), we do not create it in the origin this year.
                                }
                            } else if (thisFarmOtherInTransactions.Count() > 0)
                            {
                                foreach (var inTransaction in thisFarmOtherInTransactions)
                                {
                                    var sourceOtherProduction = previousYearAgriculturalProductions.SingleOrDefault(q => q.Id == inTransaction.ProductionId);
                                    var destinationOtherProduction = thisFarmPreviousYearOtherCropProductions.SingleOrDefault(q => q.ProductGroupId == inTransaction.Production.ProductGroupId);
                                    if (inTransaction.Percentage >= 0 && inTransaction.Percentage <= 1)
                                    {
                                        if (sourceOtherProduction != null)
                                        {
                                            if (destinationOtherProduction == null)
                                            {
                                                destinationOtherProduction = new AgriculturalProduction
                                                {
                                                    FarmId = farm.Id,
                                                    YearId = targetYear.Id,
                                                    ProductGroupId = sourceOtherProduction.ProductGroupId,
                                                    CultivatedArea = 0,
                                                    VariableCosts = 0,
                                                    CropProduction = 0,
                                                    SellingPrice = 0,
                                                    IrrigatedArea = 0,
                                                    LandValue = 0,
                                                    OrganicProductionType = 0,
                                                    QuantitySold = 0,
                                                    QuantityUsed = 0,
                                                    ValueSales = 0
                                                };
                                            }
                                            destinationOtherProduction.CultivatedArea += sourceOtherProduction.CultivatedArea * inTransaction.Percentage;
                                            destinationOtherProduction.VariableCosts = sourceOtherProduction.VariableCosts;
                                            destinationOtherProduction.CropProduction += sourceOtherProduction.CropProduction * inTransaction.Percentage;
                                            destinationOtherProduction.SellingPrice = sourceOtherProduction.SellingPrice;
                                            destinationOtherProduction.IrrigatedArea += sourceOtherProduction.IrrigatedArea * inTransaction.Percentage;
                                            destinationOtherProduction.LandValue += sourceOtherProduction.LandValue * inTransaction.Percentage;
                                            destinationOtherProduction.OrganicProductionType = sourceOtherProduction.OrganicProductionType;
                                            destinationOtherProduction.QuantitySold += sourceOtherProduction.QuantitySold * inTransaction.Percentage;
                                            destinationOtherProduction.QuantityUsed += sourceOtherProduction.QuantityUsed * inTransaction.Percentage;
                                            destinationOtherProduction.ValueSales += sourceOtherProduction.ValueSales * inTransaction.Percentage;
                                        }
                                        else
                                        {
                                            _logger.LogWarning("The source production for the transaction was not found in the previous year productions. TransactionId: " + inTransaction.Id);
                                        }
                                    } else
                                    {
                                        _logger.LogWarning($"Transference of land detected with negative or > 100% transfer value (which is not possible). ID={inTransaction.Id}");
                                    }
                                }    
                            }

                            // We translate from one year to the other category productions
                            var previousYearOtherLivestockProductions = thisFarmPreviousYearLivestockProductions.
                                Where(q => otherProductGroupsIds.Contains(q.ProductGroupId)).ToList();

                            foreach (var previousYearOtherLivestockProduction in previousYearOtherLivestockProductions)
                            {
                                thisFarmNewLivestockProductions.Add(new LivestockProduction
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    NumberOfAnimalsSold = previousYearOtherLivestockProduction.NumberOfAnimalsSold,
                                    ProductGroupId = previousYearOtherLivestockProduction.ProductGroupId,
                                    NumberOfAnimals = previousYearOtherLivestockProduction.NumberOfAnimals,
                                    NumberAnimalsForSlaughtering = previousYearOtherLivestockProduction.NumberAnimalsForSlaughtering,
                                    NumberAnimalsRearingBreading = previousYearOtherLivestockProduction.NumberAnimalsRearingBreading,
                                    DairyCows = previousYearOtherLivestockProduction.DairyCows,
                                    MilkTotalProduction = previousYearOtherLivestockProduction.MilkTotalProduction,
                                    MilkProductionSold = previousYearOtherLivestockProduction.MilkProductionSold,
                                    MilkTotalSales = previousYearOtherLivestockProduction.MilkTotalSales,
                                    MilkVariableCosts = previousYearOtherLivestockProduction.MilkVariableCosts,
                                    ManureTotalSales = previousYearOtherLivestockProduction.ManureTotalSales,
                                    ValueSoldAnimals = previousYearOtherLivestockProduction.ValueSoldAnimals,
                                    EggsTotalProduction = previousYearOtherLivestockProduction.EggsTotalProduction,
                                    EggsTotalSales = previousYearOtherLivestockProduction.EggsTotalSales,
                                    WoolTotalProduction = previousYearOtherLivestockProduction.WoolTotalProduction,
                                    EggsProductionSold = previousYearOtherLivestockProduction.EggsProductionSold,
                                    ValueAnimalsRearingBreading = previousYearOtherLivestockProduction.ValueAnimalsRearingBreading,
                                    ValueSlaughteredAnimals = previousYearOtherLivestockProduction.ValueSlaughteredAnimals,
                                    WoolProductionSold  = previousYearOtherLivestockProduction.WoolProductionSold,
                                    SellingPrice = previousYearOtherLivestockProduction.SellingPrice,
                                    VariableCosts = previousYearOtherLivestockProduction.VariableCosts
                                });
                            }


                            // We add the coupled subsidies for the other crops
                            var previosuYearOtherSubsidies = thisFarmPreviousYearSubsidies.Where(q => otherPolicyIds.Contains(q.PolicyId)).ToList();
                            foreach (var otherSubsidy in previosuYearOtherSubsidies)
                            {
                                thisFarmNewFarmYearSubsidies.Add(new FarmYearSubsidy
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    PolicyId = otherSubsidy.PolicyId,
                                    Value = otherSubsidy.Value
                                });
                            }

                            if (simulationValue.GreeningSurface > 0)
                            {
                                thisFarmNewGreeningAllocations.Add(new GreeningFarmYearData
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    GreeningSurface = simulationValue.GreeningSurface
                                });
                            }
                        }
                        else
                        {
                            var thisFarmOutTransactions = regionLandTransactions.Where(q => q.Production.FarmId == farm.Id).ToList();
                            var thisFarmInTransactions = regionLandTransactions.Where(q => q.DestinationFarmId == farm.Id).ToList();

                            if (thisFarmInTransactions.Count() == 0 && thisFarmOutTransactions.Count() == 0)
                            {
                                // If the farms has no "OTHER" agricultural production, we skip the process
                                foreach (var previousYearAgriculturalProduction in thisFarmPreviousYearCropProductions)
                                {
                                    var newAgriculturalProduction = new AgriculturalProduction
                                    {
                                        FarmId = farm.Id,
                                        YearId = targetYear.Id,
                                        ProductGroupId = previousYearAgriculturalProduction.ProductGroupId,
                                        CultivatedArea = previousYearAgriculturalProduction.CultivatedArea,
                                        VariableCosts = previousYearAgriculturalProduction.VariableCosts,
                                        CropProduction = previousYearAgriculturalProduction.CropProduction,
                                        SellingPrice = previousYearAgriculturalProduction.SellingPrice,
                                        IrrigatedArea = previousYearAgriculturalProduction.IrrigatedArea,
                                        LandValue = previousYearAgriculturalProduction.LandValue,
                                        OrganicProductionType = previousYearAgriculturalProduction.OrganicProductionType,
                                        QuantitySold = previousYearAgriculturalProduction.QuantitySold,
                                        QuantityUsed = previousYearAgriculturalProduction.QuantityUsed,
                                        ValueSales = previousYearAgriculturalProduction.ValueSales
                                    };
                                    if (!thisFarmNewAgriculturalProductions.Any(q => q.ProductGroupId == newAgriculturalProduction.ProductGroupId && q.FarmId == newAgriculturalProduction.FarmId && q.YearId == newAgriculturalProduction.YearId))
                                    {
                                        thisFarmNewAgriculturalProductions.Add(newAgriculturalProduction);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Trying to add a duplicate Agricultural production, please check the data");
                                    }
                                }
                            }
                            else if (thisFarmOutTransactions.Count() > 0)
                            {
                                foreach (var previousYearCropProduction in thisFarmPreviousYearCropProductions)
                                {
                                    var thisPreviousYearOtherCropProductionProductId = previousYearCropProduction.ProductGroupId;
                                    var sameProductTransfer = thisFarmOutTransactions.Where(q => q.Production.ProductGroupId == thisPreviousYearOtherCropProductionProductId).ToList();
                                    var totalPercentageSold = sameProductTransfer.Count > 0 ? sameProductTransfer.Sum(q => q.Percentage) : 0;

                                    if (totalPercentageSold < 1 && totalPercentageSold > 0)
                                    {
                                        var newAgriculturalProduction = new AgriculturalProduction
                                        {
                                            FarmId = farm.Id,
                                            YearId = targetYear.Id,
                                            ProductGroupId = previousYearCropProduction.ProductGroupId,
                                            CultivatedArea = previousYearCropProduction.CultivatedArea * (1 - totalPercentageSold),
                                            VariableCosts = previousYearCropProduction.VariableCosts,
                                            CropProduction = previousYearCropProduction.CropProduction * (1 - totalPercentageSold),
                                            SellingPrice = previousYearCropProduction.SellingPrice,
                                            IrrigatedArea = previousYearCropProduction.IrrigatedArea * (1 - totalPercentageSold),
                                            LandValue = previousYearCropProduction.LandValue * (1 - totalPercentageSold),
                                            OrganicProductionType = previousYearCropProduction.OrganicProductionType,
                                            QuantitySold = previousYearCropProduction.QuantitySold * (1 - totalPercentageSold),
                                            QuantityUsed = previousYearCropProduction.QuantityUsed * (1 - totalPercentageSold),
                                            ValueSales = previousYearCropProduction.ValueSales * (1 - totalPercentageSold)
                                        };
                                        if (!thisFarmNewAgriculturalProductions.Any(q => q.ProductGroupId == newAgriculturalProduction.ProductGroupId && q.FarmId == newAgriculturalProduction.FarmId && q.YearId == newAgriculturalProduction.YearId))
                                        {
                                            thisFarmNewAgriculturalProductions.Add(newAgriculturalProduction);
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Trying to add a duplicate Agricultural production, please check the data");
                                        }
                                    }
                                    // if it is == 1, we don't create it in the new year
                                }
                            }
                            else if (thisFarmInTransactions.Count() > 0)
                            {
                                foreach (var inTransaction in thisFarmInTransactions)
                                {
                                    var sourceProduction = previousYearAgriculturalProductions.SingleOrDefault(q => q.Id == inTransaction.ProductionId);
                                    var destinationProduction = thisFarmPreviousYearCropProductions.SingleOrDefault(q => q.ProductGroupId == inTransaction.Production.ProductGroupId);
                                    if (sourceProduction != null)
                                    {
                                        if (destinationProduction == null)
                                        {
                                            destinationProduction = new AgriculturalProduction
                                            {
                                                FarmId = farm.Id,
                                                YearId = targetYear.Id,
                                                ProductGroupId = sourceProduction.ProductGroupId,
                                                CultivatedArea = 0,
                                                VariableCosts = 0,
                                                CropProduction = 0,
                                                SellingPrice = 0,
                                                IrrigatedArea = 0,
                                                LandValue = 0,
                                                OrganicProductionType = 0,
                                                QuantitySold = 0,
                                                QuantityUsed = 0,
                                                ValueSales = 0
                                            };
                                        }
                                        destinationProduction.CultivatedArea += sourceProduction.CultivatedArea * inTransaction.Percentage;
                                        destinationProduction.VariableCosts = sourceProduction.VariableCosts;
                                        destinationProduction.CropProduction += sourceProduction.CropProduction * inTransaction.Percentage;
                                        destinationProduction.SellingPrice = sourceProduction.SellingPrice;
                                        destinationProduction.IrrigatedArea += sourceProduction.IrrigatedArea * inTransaction.Percentage;
                                        destinationProduction.LandValue += sourceProduction.LandValue * inTransaction.Percentage;
                                        destinationProduction.OrganicProductionType = sourceProduction.OrganicProductionType;
                                        destinationProduction.QuantitySold += sourceProduction.QuantitySold * inTransaction.Percentage;
                                        destinationProduction.QuantityUsed += sourceProduction.QuantityUsed * inTransaction.Percentage;
                                        destinationProduction.ValueSales += sourceProduction.ValueSales * inTransaction.Percentage;
                                    }
                                    else
                                    {
                                        _logger.LogWarning("The source production for the transaction was not found in the previous year productions. TransactionId: " + inTransaction.Id);
                                    }
                                }
                            }

                            
                            foreach (var previousYearLivestockProduction in thisFarmPreviousYearLivestockProductions)
                            {
                                var newLivestockProduction = new LivestockProduction
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    NumberOfAnimalsSold = previousYearLivestockProduction.NumberOfAnimalsSold,
                                    ProductGroupId = previousYearLivestockProduction.ProductGroupId,
                                    NumberOfAnimals = previousYearLivestockProduction.NumberOfAnimals,
                                    NumberAnimalsForSlaughtering = previousYearLivestockProduction.NumberAnimalsForSlaughtering,
                                    NumberAnimalsRearingBreading = previousYearLivestockProduction.NumberAnimalsRearingBreading,
                                    DairyCows = previousYearLivestockProduction.DairyCows,
                                    MilkTotalProduction = previousYearLivestockProduction.MilkTotalProduction,
                                    MilkProductionSold = previousYearLivestockProduction.MilkProductionSold,
                                    MilkTotalSales = previousYearLivestockProduction.MilkTotalSales,
                                    SellingPrice = previousYearLivestockProduction.SellingPrice,
                                    VariableCosts = previousYearLivestockProduction.VariableCosts
                                };
                                thisFarmNewLivestockProductions.Add(newLivestockProduction);
                            }

                            foreach (var subsidy in thisFarmPreviousYearSubsidies)
                            {
                                thisFarmNewFarmYearSubsidies.Add(new FarmYearSubsidy
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    PolicyId = subsidy.PolicyId,
                                    Value = subsidy.Value
                                });
                            }
                            var previousYearGreeningAllocation = farm.GreeningFarmYearData?.SingleOrDefault(q => q.YearId == previousYear.Id)?.GreeningSurface ?? 0;
                            if (previousYearGreeningAllocation > 0)
                            {
                                thisFarmNewGreeningAllocations.Add(new GreeningFarmYearData
                                {
                                    FarmId = farm.Id,
                                    YearId = targetYear.Id,
                                    GreeningSurface = previousYearGreeningAllocation
                                });
                            }
                        }

                        // After getting the productions, update all influenced closingValues
                        var totalCosts =
                            thisFarmNewAgriculturalProductions.Sum(ap => ap.VariableCosts ?? 0 * (ap.QuantityUsed + ap.QuantitySold) ?? 0) +
                            thisFarmNewLivestockProductions.Sum(lp => lp.VariableCosts * (lp.EggsTotalProduction + lp.MilkTotalProduction + lp.WoolTotalProduction));
                        var totalSales =
                            thisFarmNewAgriculturalProductions.Sum(ap => ap.ValueSales ?? 0) +
                            thisFarmNewLivestockProductions.Sum(lp => lp.MilkTotalSales + lp.ManureTotalSales + lp.ValueSoldAnimals + lp.EggsTotalSales);
                        var totalSubsidies = thisFarmNewFarmYearSubsidies.Sum(fys => fys.Value);
                        var totalRents =
                            newLandRents?.Where(q => q.OriginFarmId == farm.Id).Sum(q => q.RentValue) ?? 0 
                            - newLandRents?.Where (q => q.DestinationFarmId == farm.Id).Sum(q => q.RentValue) ?? 0;
                        // We suppose this is = Gross Marging althoug gross margin and FNVA is recalculated after
                        thisYearClosingValue.GrossFarmIncome = totalSales + totalSubsidies - totalCosts;
                        thisYearClosingValue.Taxes = previousYearClosingValue.GrossFarmIncome != 0 ?
                            (previousYearClosingValue.Taxes * simulationValue?.FarmGrossIncome / previousYearClosingValue.GrossFarmIncome ?? previousYearClosingValue.Taxes) :
                            (averageTaxGrossIncomeRate * simulationValue?.FarmGrossIncome ?? previousYearClosingValue.Taxes);
                        thisYearClosingValue.FarmNetIncome = thisYearClosingValue.GrossFarmIncome + thisYearClosingValue.Taxes - thisYearClosingValue.Depreciation;
                        thisYearClosingValue.VatBalanceExcludingInvestments = previousYearClosingValue.GrossFarmIncome != 0 ?
                            (previousYearClosingValue.VatBalanceExcludingInvestments * simulationValue?.FarmGrossIncome / previousYearClosingValue.GrossFarmIncome ?? previousYearClosingValue.VatBalanceExcludingInvestments) :
                            (averageVatGrossIncomeRate * simulationValue?.FarmGrossIncome ?? previousYearClosingValue.VatBalanceExcludingInvestments);

                        // We update the current Assets adding the part of the "OTHER" productions only if there was not a simulationValue for the farm. we add also the subsidies for the OTHER crops
                        if (simulationValue != null)
                        {
                            thisYearClosingValue.TotalCurrentAssets = thisYearClosingValue.TotalCurrentAssets
                                + thisFarmNewAgriculturalProductions.Where(ap => otherProductGroupsIds.Contains(ap.ProductGroupId ?? 0)).
                                    Sum(ap => (ap.ValueSales ?? 0 - ap.VariableCosts ?? 0 * (ap.QuantitySold + ap.QuantityUsed)) ?? 0)
                                + thisFarmNewLivestockProductions.Where(lp => otherProductGroupsIds.Contains(lp.ProductGroupId)).
                                    Sum(lp => (lp.MilkTotalSales  - lp.MilkVariableCosts * lp.MilkTotalProduction) + (lp.ManureTotalSales + lp.EggsTotalSales + lp.ValueSoldAnimals - lp.VariableCosts * (lp.EggsTotalProduction + lp.WoolTotalProduction)))
                                + thisFarmNewFarmYearSubsidies.Where(q => otherPolicyIds.Contains(q.PolicyId)).Sum(q => q.Value)
                                + totalRents;
                        }
                        thisYearClosingValue.TotalOutputCropsAndCropProduction = thisFarmNewAgriculturalProductions.Sum(ap => ap.CropProduction ?? 0);
                        thisYearClosingValue.TotalOutputLivestockAndLivestockProduction = thisFarmNewLivestockProductions.Sum(q => q.MilkTotalProduction + q.EggsTotalProduction + q.WoolTotalProduction);

                        //we update the closingValFarmValues row for year+1
                        newClosingValFarmValues.Add(thisYearClosingValue);
                        newAgriculturalProductions.AddRange(thisFarmNewAgriculturalProductions);
                        newLivestockProductions.AddRange(thisFarmNewLivestockProductions);
                        newFarmYearSubsidies.AddRange(thisFarmNewFarmYearSubsidies);
                        newGreeningAllocations.AddRange(thisFarmNewGreeningAllocations);
                    }

                    // Fix to avoid Agricultural Productions with 0 cultivated Area:
                    newAgriculturalProductions.RemoveAll(q => q.CultivatedArea == 0);

                    if (newAgriculturalProductions.Any())
                    {
                        var (successA, errorMessageA) = await _repositoryAgriculturalProduction.AddRangeAsync(newAgriculturalProductions);
                        if (successA == false)
                        {
                            // Let's find the repeated ones:
                            var repeatedAgriculturalProductions = newAgriculturalProductions.GroupBy(q => new { q.FarmId, q.ProductGroupId, q.YearId }).Where(q => q.Count() > 1).Select(q => q.Key).ToList();
                            _logger.LogError("Error while inserting AgriculturalProductions: " + errorMessageA);
                            return BadRequest(errorMessageA);
                        }
                    }
                    if (newLivestockProductions.Any())
                    {
                        var (successL, errorMessageL) = await _repositoryLivestockProduction.AddRangeAsync(newLivestockProductions);
                        if (successL == false)
                        {
                            _logger.LogError("Error while inserting LivestockProductions: " + errorMessageL);
                            return BadRequest(errorMessageL);
                        }
                    }
                    if (newClosingValFarmValues.Any())
                    {
                        var (successC, errorMessageC) = await _repositoryClosingValFarmValue.AddRangeAsync(newClosingValFarmValues);
                        if (successC == false)
                        {
                            _logger.LogError("Error while inserting ClosingValFarmValues: " + errorMessageC);
                            return BadRequest(errorMessageC);
                        }
                    }
                    if (newFarmYearSubsidies.Any())
                    {
                        var (successF, errorMessageF) = await _repositoryFarmYearSubsidy.AddRangeAsync(newFarmYearSubsidies);
                        if (successF == false)
                        {
                            _logger.LogError("Error while inserting FarmYearSubsidies: " + errorMessageF);
                            return BadRequest(errorMessageF);
                        }
                    }
                    if (newGreeningAllocations.Any())
                    {
                        var (successF, errorMessageF) = await _repositoryGreeningFarmYear.AddRangeAsync(newGreeningAllocations);
                        if (successF == false)
                        {
                            _logger.LogError("Error while inserting GreeningFarmYear: " + errorMessageF);
                            return BadRequest(errorMessageF);
                        }
                    }
                    if (newLandRents?.Any() ?? false)
                    {
                        var (successF, errorMessageF) = await _repositoryLandRent.AddRangeAsync(newLandRents);
                        if (successF == false)
                        {
                            _logger.LogError("Error while inserting LandRent: " + errorMessageF);
                            return BadRequest(errorMessageF);
                        }
                    }

                    var (result, message) = await _jsonObjService.FixNetIncomeAndGrossMarginCalculation(regionFarms.Select(q => q.Id).ToList(), new List<long> { year });
                    if (!result)
                    {
                        _logger.LogError("Error while updating the Farm net income and gross margint: " + message);
                        return BadRequest(message);
                    }
                }

                return StatusCode(201);
            } catch (Exception ex)
            {
                //Raise ex exception
                _logger.LogError("Error while inserting data: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}


