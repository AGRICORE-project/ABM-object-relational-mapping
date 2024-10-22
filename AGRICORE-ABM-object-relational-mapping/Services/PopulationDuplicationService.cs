using CsvHelper.Configuration;
using CsvHelper;
using DB.Data.Models;
using DB.Data.Repositories;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DB.Data.DTOs;

namespace AGRICORE_ABM_object_relational_mapping.Services
{
    /// <summary>
    /// Service for duplicating populations and associated data for simulations.
    /// </summary>
    public interface IPopulationDuplicationService
    {
        Task<long> CreatePopulationForSimulation(long syntheticPopulationId);
    }


    public class PopulationDuplicationService : IPopulationDuplicationService
    {
        private readonly IRepository<Population> _populationRepository;
        private readonly IRepository<SyntheticPopulation> _syntheticPopulationRepository;
        private readonly IRepository<Farm> _farmRepository;
        private readonly IJsonObjService _jsonObjService;
        private readonly ILogger<PopulationDuplicationService> _logger;

        /// <summary>
        /// Constructor for PopulationDuplicationService.
        /// </summary>
        /// <param name="populationRepository">Repository for populations.</param>
        /// <param name="syntheticPopulationRepository">Repository for synthetic populations.</param>
        /// <param name="farmRepository">Repository for farms.</param>
        /// <param name="jsonObjService">Service for JSON object operations.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public PopulationDuplicationService(
                    IRepository<Population> populationRepository,
                    IRepository<SyntheticPopulation> syntheticPopulationRepository, 
                    IRepository<Farm> farmRepository,
                    IJsonObjService jsonObjService,
                    ILogger<PopulationDuplicationService> logger
               )
        {
            _populationRepository = populationRepository;
            _syntheticPopulationRepository = syntheticPopulationRepository;
            _farmRepository = farmRepository;
            _jsonObjService = jsonObjService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new population for simulation by duplicating data from a synthetic population.
        /// </summary>
        /// <param name="syntheticPopulationId">ID of the synthetic population to duplicate.</param>
        /// <returns>ID of the newly created population.</returns>
        public async Task<long> CreatePopulationForSimulation(long syntheticPopulationId)
        {
            long newPopulationId = 0;
            var sp = await _syntheticPopulationRepository.GetSingleOrDefaultAsync(sp => sp.Id == syntheticPopulationId);
            if (sp != null)
            {

                _logger.LogInformation($"Duplicating basic population structure");
                SyntheticPopulationJsonDTO spJson = null;
                (spJson, var _) = await _jsonObjService.ExportSyntheticPopulation(syntheticPopulationId, includeFarms: false, includePoliciesAndProductGroups: true, includeTransactions: false);
                
                if (spJson != null)
                {
                    newPopulationId = await _jsonObjService.ImportPopulationFromSPJson(spJson, fixIncomeAndGM: false);
                    var newPopulation = await _populationRepository.GetSingleOrDefaultAsync(p => p.Id == newPopulationId, include: p => p
                        .Include(p => p.ProductGroups)
                        .Include(p => p.PolicyGroupRelations)
                        .Include(p => p.Policies)
                        .Include(p => p.FADNProductRelations)
                        .Include(p => p.Years)
                        .Include(p => p.ProductGroups),
                        asNoTracking: true,
                        asSeparateQuery: true
                    );
                    if (newPopulation != null)
                    {
                        newPopulation.Description = $"(Replicated from SP: {sp.Name} - {newPopulation.Description} - {newPopulationId})";
                        _populationRepository.Update(newPopulation);
                        _logger.LogInformation($"Including farms.");
                        int countFarms = await _farmRepository.CountAsync(f => f.PopulationId == sp.PopulationId);
                        int remainingFarms = countFarms;
                        int batchSize = 2500;
                        int batchCount = 0;
                        long lastProcessedFarmId = 0;
                        List<Dictionary<string, long>> batchDictionaries = new List<Dictionary<string, long>>();
                        Dictionary<string,long> currentFarmCodeDictionary = new Dictionary<string, long>();

                        if (countFarms > batchSize)
                        {
                            _logger.LogInformation($"There are more than {batchSize} farms in the population ({countFarms}). It will be addressed by batches of {batchSize} farms");
                        }

                        while (remainingFarms > 0)
                        {
                            (spJson, currentFarmCodeDictionary) = await _jsonObjService.ExportSyntheticPopulation(
                                syntheticPopulationId, 
                                limitFarms: batchSize, 
                                minimumIdToInclude: lastProcessedFarmId, 
                                includeFarms: true, 
                                includePoliciesAndProductGroups: false,
                                includeTransactions: false
                                );
                            _logger.LogInformation($"Bath {batchCount} exporting concluded. Farms included: {currentFarmCodeDictionary.Count}");
                            batchDictionaries.Add(currentFarmCodeDictionary);
                            lastProcessedFarmId = currentFarmCodeDictionary.Select(kvp => kvp.Value).Max();
                            await _jsonObjService.ImportPartialFarmsIntoPopulationFromJson(newPopulationId, spJson.Population.Farms, newPopulation);
                            _logger.LogInformation($"Bath {batchCount} importing concluded. ");

                            remainingFarms = remainingFarms - batchSize;
                            remainingFarms = (remainingFarms < 0 ? 0 : remainingFarms);
                            batchCount++;

                            _logger.LogInformation($"Batches completed: {batchCount}. Remaining Farms {remainingFarms}");
                        }
                        _logger.LogInformation($"Importing rents and land transfers");

                        (spJson, var _) = await _jsonObjService.ExportSyntheticPopulation(syntheticPopulationId, batchSize, lastProcessedFarmId, includeFarms: false, includePoliciesAndProductGroups: false, includeTransactions: true);
                        Dictionary<string, long> mergedDictionary = new Dictionary<string, long>();
                        foreach (var dictionary in batchDictionaries)
                        {
                            foreach (var kvp in dictionary)
                            {
                                // We are assuming that the keys are unique, as they should
                                mergedDictionary[kvp.Key] = kvp.Value;
                            }
                        }
                        await _jsonObjService.ImportLandRentsFromJson(newPopulation.Id, spJson.Population.LandRents, mergedDictionary);
                        await _jsonObjService.ImportLandTransactionsFromJson(newPopulation.Id, spJson.Population.LandTransactions, mergedDictionary);
                        _logger.LogInformation($"Duplication completed");
                    }
                    else
                    {
                        _logger.LogError($"Replication of policy could not be done");
                    }
                }
            }
            return newPopulationId;
        }
    }
}
