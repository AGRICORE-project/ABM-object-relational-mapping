using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGRICORE_ABM_object_relational_mapping.Helpers;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing long period simulation data related to population, farms, and agricultural management decisions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LongPeriodController : ControllerBase
    {
        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<ClosingValFarmValue> _repositoryClosingValFarmValue;
        private readonly IRepository<AgroManagementDecision> _repositoryAgroManagementDecision;
        private readonly IRepository<LandTransaction> _repositoryLandTransaction;
        private readonly IRepository<LandRent> _repositoryLandRents;
        private readonly IRepository<HolderFarmYearData> _repositoryHolderFarmYearData;
        private readonly IMapper _mapper;
        private readonly ILogger<LongPeriodController> _logger;


        public LongPeriodController(
            IRepository<Population> repositoryPopulation,
            IRepository<Farm> repositoryFarm,
            IRepository<Year> repositoryYear,
            IRepository<ClosingValFarmValue> repositoryClosingValFarmValue,
            IRepository<AgroManagementDecision> repositoryAgroManagementDecision,
            IRepository<LandTransaction> repositoryLandTransaction,
            IRepository<HolderFarmYearData> repositoryHolderFarmYearData,
            IRepository<LandRent> repositoryLandRent,
            IMapper mapper,
            ILogger<LongPeriodController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryFarm = repositoryFarm;
            _repositoryYear = repositoryYear;
            _repositoryClosingValFarmValue = repositoryClosingValFarmValue;
            _repositoryAgroManagementDecision = repositoryAgroManagementDecision;
            _repositoryLandTransaction = repositoryLandTransaction;
            _repositoryHolderFarmYearData = repositoryHolderFarmYearData;
            _repositoryLandRents = repositoryLandRent;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves data for long period simulation based on population ID and year.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <param name="year">Year for which data is requested.</param>
        /// <param name="ignoreLP">Flag to ignore LP.</param>
        /// <param name="ignoreLMM">Flag to ignore LMM.</param>
        /// <returns>Data required for long period simulation.</returns>


        [HttpGet("/population/{populationId}/farms/get/simulationdata/longperiod")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DataToLPDTO>> GetDataForLongPeriodSimulation(long populationId, [FromQuery] int year, [FromQuery] bool ignoreLP = false, [FromQuery] bool ignoreLMM = false)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
            .Include(p => p.Years.Where(y => y.YearNumber == year-1))
            .Include(p => p.Farms).ThenInclude(p => p.AgriculturalProductions.Where(a => a.Year.YearNumber == year - 1))
            .Include(p => p.Farms).ThenInclude(p => p.ClosingValFarmValues.Where(c => c.Year.YearNumber == year-1))
            .Include(p => p.Farms).ThenInclude(p => p.HoldersFarmYearData.Where(c => c.Year.YearNumber == year-1)).ThenInclude(h => h.Year)
            .Include(p => p.Farms).ThenInclude(p => p.FarmYearSubsidies.Where(c => c.Year.YearNumber == year - 1)).ThenInclude(f => f.Year)
            .Include(p => p.Farms).ThenInclude(p => p.FarmYearSubsidies.Where(c => c.Year.YearNumber == year - 1)).ThenInclude(f => f.Policy)
            .Include(p => p.Policies).ThenInclude(p => p.PolicyGroupRelations).ThenInclude(p => p.ProductGroup),
            asSeparateQuery:true, asNoTracking:true);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(409, error);
            }

            var existingYear = existingPopulation.Years.SingleOrDefault();

            if (existingYear == null)
            {
                error = "This year does not exist for this population";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            if (existingPopulation.Farms == null || existingPopulation.Farms.Count == 0)
            {
                error = "There are no farms for this population";
                _logger.LogError(error);
                return StatusCode(404, error);
            }

            List<ClosingValFarmValue> farmsData = new List<ClosingValFarmValue>();
            List<FarmYearSubsidy> farmsSubsidies = new List<FarmYearSubsidy>();
            List<HolderFarmYearData> farmsHolders = new List<HolderFarmYearData>();
            List<AgriculturalProduction> agriculturalProductions = new List<AgriculturalProduction>();
            foreach (Farm farm in existingPopulation.Farms)
            {
                farmsData.Add(farm.ClosingValFarmValues.SingleOrDefault());
                farmsSubsidies.AddRange(farm.FarmYearSubsidies);
                if(farm.HoldersFarmYearData.Count > 0)
                    farmsHolders.Add(farm.HoldersFarmYearData.SingleOrDefault());
                
                if (farm.AgriculturalProductions == null)
                    continue;
                agriculturalProductions.AddRange(farm.AgriculturalProductions);
                
            }
            Dictionary<long, long> dictRegions = new Dictionary<long, long>();
            if (existingPopulation.Farms != null && existingPopulation.Farms.Count >0)
                dictRegions = existingPopulation.Farms.ToDictionary(f => f.Id, f => f.RegionLevel3);
            var mappedAgriculturalProductions = _mapper.Map<List<AgriCulturalProductionDTO>>(agriculturalProductions);
            var mappedValues = farmsData
                .Select(data => new ValueToLPDTO
                {
                    SE465 = data.TotalCurrentAssets,
                    SE420 = data.FarmNetIncome,
                    SE410 = data.GrossFarmIncome,
                    SE490 = data.LongAndMediumTermLoans,
                    AgriculturalLandValue = data.AgriculturalLandValue,
                    AgriculturalLandArea = data.AgriculturalLandArea,
                    FarmId = data.FarmId,
                    AverageHAPrice = data.AgriculturalLandArea > 0 ? data.AgriculturalLandValue / data.AgriculturalLandArea : 0,
                    AversionRiskFactor = 0.5f,
                    AgentHolder = _mapper.Map<HolderFarmYearDataJsonDTO>(farmsHolders.Find(h => h.FarmId == data.FarmId)),
                    AgentSubsidies = _mapper.Map<List<FarmYearSubsidyDTO>>(farmsSubsidies.FindAll(f => f.FarmId == data.FarmId)),
                    RegionLevel3 = dictRegions[data.FarmId]
                }

            ).ToList();
            var pgr = _mapper.Map<List<PolicyGroupRelationJsonDTO>>(existingPopulation.PolicyGroupRelations);

            var policies = _mapper.Map<List<PolicyJsonDTO>>(existingPopulation.Policies);

            var rentOperations = await _repositoryLandRents.GetAllAsync(q => q.Year.YearNumber == year - 1, include: q => q.Include(q => q.Year).Include(q => q.OriginFarm).Include(q => q.DestinationFarm), asNoTracking: true);
            var mappedRentOperations = _mapper.Map<List<LandRentDTO>>(rentOperations);

            DataToLPDTO dataToLPDTO = new DataToLPDTO
            {
                Values = mappedValues,
                AgriculturalProductions = mappedAgriculturalProductions,
                IgnoreLP = ignoreLP,
                IgnoreLMM = ignoreLMM,
                PolicyGroupRelations = pgr,
                Policies = policies,
                RentOperations = mappedRentOperations
            };

            if (mappedValues.Count > 0)
            {
                return Ok(dataToLPDTO);
            }
            else
            {
                error = "No data for the given year.";
                _logger.LogError(error);
                return BadRequest(error);
            }
        }

        /// <summary>
        /// Adds agricultural management decisions from long period simulation results.
        /// </summary>
        /// <param name="data">Data containing agricultural management decisions.</param>
        /// <returns>Result of adding agricultural management decisions.</returns>

        [HttpPost("/results/longperiod")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> AddValuesFromLP(AgroManagementDecisionFromLP data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError(ErrorHelper.GetErrorDescription(ModelState));
                    return BadRequest(ModelState);
                }
                string error = string.Empty;
                if (data.AgroManagementDecisions.Count == 0)
                {
                    error = "No data to add";
                    _logger.LogError(error);
                    return BadRequest(error);
                }


                List<AgroManagementDecision> agroManagementDecisions = _mapper.Map<List<AgroManagementDecision>>(data.AgroManagementDecisions);

                var years = await _repositoryYear.GetAllAsync(predicate: y => agroManagementDecisions.Select(v => v.YearId).Contains(y.Id), asNoTracking: true);
                // All AgroManagementDecision should be from the same year
                if (years == null || years.Count != 1)
                {
                    error = "Some values are from different years or the year does not exist";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
                // if this is a re-run we delete the previous run results
                List<AgroManagementDecision> existingDecisions = await _repositoryAgroManagementDecision.GetAllAsync(ma => ma.YearId == years[0].Id);
                if (existingDecisions.Any())
                    _repositoryAgroManagementDecision.RemoveRange(existingDecisions);

                var previousYear = await _repositoryYear.GetSingleOrDefaultAsync(predicate: y => y.YearNumber == years[0].YearNumber - 1 && y.PopulationId == years[0].PopulationId, asNoTracking: true);

                var allFarms = await _repositoryFarm.GetAllAsync(predicate: f => f.PopulationId == years[0].PopulationId, include: q => q.Include(q => q.HoldersFarmYearData).Include(q => q.ClosingValFarmValues), asNoTracking: true);
                var includedFarmIds = agroManagementDecisions.Select(v => v.FarmId).Distinct();
                
                var farmsWithDecissions = allFarms.Where(q => includedFarmIds.Contains(q.Id)).ToList();

                // For each farm with an error, we need to create an AgromanagementDecision wich equals to don't change from previous year
                var farmsWithErrors = allFarms.Where(q => data.errorList.Contains(q.Id)).ToList();
                foreach (var farm in farmsWithErrors)
                {
                    if (farm.FarmCode == "2602008011403000001")
                    {
                        Console.WriteLine("Error in farm 2602008011403000001");
                    }
                    var cv = farm.ClosingValFarmValues.SingleOrDefault(q => q.YearId == previousYear.Id);
                    agroManagementDecisions.Add(new AgroManagementDecision
                    {
                        FarmId = farm.Id,
                        YearId = years[0].Id,
                        AgriculturalLandArea = cv.AgriculturalLandArea,
                        AgriculturalLandValue = cv.AgriculturalLandValue,
                        TotalCurrentAssets = cv.TotalCurrentAssets,
                        LongAndMediumTermLoans = cv.LongAndMediumTermLoans,
                        AverageLandValue = cv.AgriculturalLandArea > 0 ? cv.AgriculturalLandValue / cv.AgriculturalLandArea : 0,
                        TargetedLandAquisitionArea = 0,
                        TargetedLandAquisitionHectarPrice = 0,
                        RetireAndHandOver = false,
                    });
                }

                // there should be a farm for each AgroManagementDecision returned by LP
                if (allFarms == null || allFarms.Count() != agroManagementDecisions.Count)
                {
                    error = "Not all farms make decissions even after automatic decission correction";
                    _logger.LogError(error);
                    return BadRequest(error);
                }
                if (years[0].PopulationId != farmsWithDecissions[0].PopulationId)
                {
                    error = "This population does not contains some of the farms for this year";
                    _logger.LogError(error);
                    return BadRequest(error);
                }

                // if this is a re-run we delete the previous run results
                List<LandTransaction> existingTransactions = await _repositoryLandTransaction.GetAllAsync(lt => lt.YearId == years[0].Id);
                if (existingTransactions.Any())
                    _repositoryLandTransaction.RemoveRange(existingTransactions);

                List<HolderFarmYearData> existingHolders = await _repositoryHolderFarmYearData.GetAllAsync(h => h.YearId == years[0].Id);
                if (existingHolders.Any())
                    _repositoryHolderFarmYearData.RemoveRange(existingHolders);

                var (success, errorMessage) = await _repositoryAgroManagementDecision.AddRangeAsync(agroManagementDecisions);
                if (success)
                {
                    List<LandTransaction> landtransactions = _mapper.Map<List<LandTransaction>>(data.LandTransactions);
                    var createdAgroManagementDecisons = agroManagementDecisions.Select(x => x.Id).ToList();
                    _logger.LogInformation($"AgroManagementDecision {String.Join(",", createdAgroManagementDecisons)} added");
                    if (landtransactions.Count > 0)
                    {
                        (success, errorMessage) = await _repositoryLandTransaction.AddRangeAsync(landtransactions);
                        if (success)
                        {
                            var createdLandTransactions = landtransactions.Select(x => x.Id).ToList();
                            _logger.LogInformation($"Landtransactions {String.Join(",", createdLandTransactions)} added");
                        }
                        else
                        {
                            _logger.LogError("Error while inserting LandTransaction: " + errorMessage);
                            return BadRequest(errorMessage); // Returning the error message in the response.
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No transactions received: " + errorMessage);
                        success = true;
                    }

                    if (success)
                    {
                        // Now we need to create the new holder FarmYearData for the new year
                        List<HolderFarmYearData> newHolderData = new List<HolderFarmYearData>();
                        foreach (var farm in allFarms)
                        {
                            var existingHolderFarmYearData = farm.HoldersFarmYearData.SingleOrDefault(h => h.YearId == previousYear.Id);
                            var thisFarmDecision = agroManagementDecisions.SingleOrDefault(a => a.FarmId == farm.Id);
                            var succession = false;
                            if (thisFarmDecision != null && thisFarmDecision.RetireAndHandOver)
                                succession = true;
                            if (succession && existingHolderFarmYearData.HolderSuccessors > 0)
                            {
                                newHolderData.Add(
                                    new HolderFarmYearData
                                    {
                                        FarmId = farm.Id,
                                        YearId = years[0].Id,
                                        HolderAge = existingHolderFarmYearData.HolderSuccessorsAge + 1,
                                        HolderFamilyMembers = existingHolderFarmYearData.HolderFamilyMembers,
                                        // This is incorrect, but we don't have the data for the successor gender
                                        HolderGender = existingHolderFarmYearData.HolderGender,
                                        HolderSuccessors = existingHolderFarmYearData.HolderSuccessors - 1,
                                        HolderSuccessorsAge = existingHolderFarmYearData.HolderSuccessorsAge + 1,
                                    }
                                    );
                            }
                            else
                            {
                                newHolderData.Add(
                                    new HolderFarmYearData
                                    {
                                        FarmId = farm.Id,
                                        YearId = years[0].Id,
                                        HolderAge = existingHolderFarmYearData.HolderAge + 1,
                                        HolderFamilyMembers = existingHolderFarmYearData.HolderFamilyMembers,
                                        HolderGender = existingHolderFarmYearData.HolderGender,
                                        HolderSuccessors = existingHolderFarmYearData.HolderSuccessors,
                                        HolderSuccessorsAge = existingHolderFarmYearData.HolderSuccessorsAge > 0 ? existingHolderFarmYearData.HolderSuccessorsAge + 1 : 0,
                                    }
                                    );
                            }
                        }
                        (success, errorMessage) = await _repositoryHolderFarmYearData.AddRangeAsync(newHolderData);
                        if (success)
                        {

                            var createdHolderFarmYearData = newHolderData.Select(x => x.Id).ToList();
                            _logger.LogInformation($"HolderFarmYearData {String.Join(",", createdHolderFarmYearData)} added");
                            return CreatedAtAction(nameof(AddValuesFromLP), new { }, new { agroManagementDecisions, landtransactions });
                        }
                    }
                }
                _logger.LogError("Error while inserting AgroManagementDecision: " + errorMessage);
                return BadRequest(errorMessage); // Returning the error message in the response.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while inserting Results From LP");
                return BadRequest(e.Message);
            }
        }
    }
}
