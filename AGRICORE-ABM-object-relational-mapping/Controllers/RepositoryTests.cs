using DB.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DB;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using DB.Data.Repositories;
using Microsoft.EntityFrameworkCore.Internal;
using Swashbuckle.AspNetCore.Annotations;
using AGRICORE_ABM_object_relational_mapping.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using DB.Data.DTOs;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Diagnostics.Metrics;
using System.ComponentModel;
using System.Collections.Generic;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepositoryTests : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<Farm> _repositoryFarm;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly IRepository<ClosingValFarmValue> _repositoryClosingValFarmValue;
        private readonly IRepository<FarmYearSubsidy> _repositoryFarmYearSubsidy;
        private readonly IRepository<LivestockProduction> _repositoryLivestockProduction;
        private readonly IRepository<AgriculturalProduction> _repositoryAgriculturalProduction;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IServiceProvider _serviceProvider;
        private readonly IArableService _arableService;
        private readonly IRepository<FADNProductRelation> _repositoryFADNProductRelation;
        private readonly IMapper _mapper;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly IRepository<SyntheticPopulation> _repositorySyntheticPopulation;
        private readonly IRepository<SimulationScenario> _repositorySimulationScenario;
        private readonly IRepository<SimulationRun> _repositorySimulationRun;
        private readonly IRepository<LogMessage> _repositoryLogMessage;
        private readonly IRepository<HolderFarmYearData> _repositoryHolderFarmYearData;
        private readonly ILogger<RepositoryTests> _logger;

        public RepositoryTests(IRepository<Farm> repositoryFarm,
                                IRepository<Year> repositoryYear,
                                IRepository<Policy> repositoryPolicy,
                                IRepository<FADNProduct> repositoryFADNProduct,
                                IRepository<ClosingValFarmValue> repositoryClosingValFarmValue,
                                IRepository<FarmYearSubsidy> repositoryFarmYearSubsidy,
                                IRepository<LivestockProduction> repositoryLivestockProduction,
                                IRepository<AgriculturalProduction> repositoryAgriculturalProduction,
                                IRepository<ProductGroup> repositoryProductGroup,
                                IServiceProvider serviceProvider,
                                IArableService arableService,
                                IRepository<FADNProductRelation> repositoryFADNProductRelation,
                                IMapper mapper,
                                IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
                                IRepository<Population> repositoryPopulation, 
                                IRepository<SyntheticPopulation> repositorySyntheticPopulation,
                                IRepository<SimulationScenario> repositorySimulationScenario,
                                IRepository<SimulationRun> repositorySimulationRun,
                                IRepository<LogMessage> repositoryLogMessage,
                                ILogger<RepositoryTests> logger,
                                IRepository<HolderFarmYearData> repositoryholderFarmYearData)
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryFarm = repositoryFarm;
            _repositoryYear = repositoryYear;
            _repositoryPolicy = repositoryPolicy;
            _repositoryFADNProduct = repositoryFADNProduct;
            _repositoryClosingValFarmValue = repositoryClosingValFarmValue;
            _repositoryFarmYearSubsidy = repositoryFarmYearSubsidy;
            _repositoryLivestockProduction = repositoryLivestockProduction;
            _repositoryAgriculturalProduction = repositoryAgriculturalProduction;
            _repositoryProductGroup = repositoryProductGroup;
            _serviceProvider = serviceProvider;
            _arableService = arableService;
            _repositoryFADNProductRelation = repositoryFADNProductRelation;
            _mapper = mapper;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _repositorySyntheticPopulation = repositorySyntheticPopulation;
            _repositorySimulationScenario = repositorySimulationScenario;
            _repositorySimulationRun = repositorySimulationRun;
            _repositoryLogMessage = repositoryLogMessage;
            _repositoryHolderFarmYearData = repositoryholderFarmYearData;
            _logger = logger;
        }

        [HttpPut("/agents/transferdata/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferData([FromQuery] long originalAgent, [FromQuery] long targetAgent, [FromQuery] int year, [FromQuery] long ha)
        {
            return Ok("todo");
        }

       
        /// <summary>
        /// This endpoint makes a filter on the data taking into account that the LP model does not stand TotalCurrentAssets=0 values. 
        /// It also looks for negative values in GrossFarmIncome and FarmNetIncom and converts them into positive. 
        /// If TotalCurrentAssets=0 it is given a value that is 0.7 times the value from GrossFarmIncome. It also holds a counter to give an idea of the number of entities modified.
        /// </summary>
        /// <returns></returns>
        [HttpPut("/filterData")]
        public async Task<IActionResult> FilterData()
        {
            var counter = 0;
            // Let's remove any farm that has 0 productiveArea to check if this fix the SP execution
            var farms = await _repositoryFarm.GetAllAsync(include: q=> q.Include(q => q.AgriculturalProductions));

            foreach (var farm in farms)
            {
                if (farm.AgriculturalProductions.Sum(ap => ap.CultivatedArea) == 0)
                {
                    _logger.LogWarning($"FilterSystem: Farm with id {farm.Id} has 0 cultivated area, removing it from the database");
                    _repositoryFarm.Remove(farm);
                    counter++;
                }
            }

            var dataList = await _repositoryClosingValFarmValue.GetAllAsync(include: ob => ob.Include(ob => ob.Farm));
            var agriProductions = await _repositoryAgriculturalProduction.GetAllAsync();

            //it ends with the Nan and Infinity problems in the csv files
            foreach (var agriProduction in agriProductions)
            {
                if (float.IsNaN((float)agriProduction.CropProduction) || float.IsInfinity((float)agriProduction.CropProduction))
                {
                    _logger.LogWarning($"FilterSystem: AgriculturalProduction with id {agriProduction.Id} has NaN or Infinity values in the CropProduction, setting it to 0");
                    agriProduction.CropProduction = 0;
                    counter++;
                    _repositoryAgriculturalProduction.Update(agriProduction);
                }
            }
            //it ends with aberrant values for the LP
            foreach (var data in dataList)
            {
                var update = false;
                if (data.GrossFarmIncome < 0)
                {
                    _logger.LogWarning($"FilterSystem: ClosingValFarmValue with id {data.Id} has a negative GrossFarmIncome, setting it to positive");
                    data.GrossFarmIncome = Math.Abs(data.GrossFarmIncome);
                    update = true;
                }
                if (data.FarmNetIncome < 0)
                {
                    _logger.LogWarning($"FilterSystem: ClosingValFarmValue with id {data.Id} has a negative FarmNetIncome, setting it to positive");
                    data.FarmNetIncome = Math.Abs(data.FarmNetIncome);
                    update = true;
                }
                if (data.TotalCurrentAssets <= 0)
                {
                    _logger.LogWarning($"FilterSystem: ClosingValFarmValue with id {data.Id} has a TotalCurrentAssets equal or less than 0, setting it to 0.7 times the GrossFarmIncome");
                    data.TotalCurrentAssets = data.GrossFarmIncome * 0.7f;
                    update = true;
                }
                if (data.AgriculturalLandValue == 0)
                {
                    _logger.LogWarning($"FilterSystem: ClosingValFarmValue with id {data.Id} has a AgriculturalLandValue equal to 0, setting it to 0.7 times the GrossFarmIncome");
                    data.AgriculturalLandValue = data.GrossFarmIncome * 0.7f;
                    update = true;
                }
                if (update == true)
                {
                    _repositoryClosingValFarmValue.Update(data);
                    counter++;
                }
            }
            return Ok("Number of entities edited from database: " + counter);
        }

        [HttpDelete("/delete")]
        public async Task<IActionResult> DeleteFarm([FromQuery] long farmId)
        {
            var farm = await _repositoryFarm.GetSingleOrDefaultAsync(f => f.Id == farmId);
            if (farm != null)
            {
                _repositoryFarm.Remove(farm);
                return Ok("Farm with id: " + farmId + ", removed");
            }
            else
            {
                return NotFound("FarmId not found");
            }
        }

         [HttpDelete("/policies/delete")]
        public async Task<IActionResult> DeleteAllPolicies()
        {
            try
            {
                await _repositoryPolicy.DeleteAllAsync();
                return Ok("All policies have been deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }



        [HttpDelete("/dumpData")]
        public async Task<IActionResult> DumpAllData()
        {
            //TO-DO: decouple this method from future repositories increase maybe using reflection?
            try
            {
                await _repositoryFarm.DeleteAllAsync();
                await _repositoryYear.DeleteAllAsync();
                await _repositoryPolicy.DeleteAllAsync();
                await _repositoryFADNProduct.DeleteAllAsync();
                await _repositoryClosingValFarmValue.DeleteAllAsync();
                await _repositoryFarmYearSubsidy.DeleteAllAsync();
                await _repositoryLivestockProduction.DeleteAllAsync();
                await _repositoryAgriculturalProduction.DeleteAllAsync();
                await _repositoryProductGroup.DeleteAllAsync();
                await _repositoryFADNProductRelation.DeleteAllAsync();
                await _repositoryPolicyGroupRelation.DeleteAllAsync();
                await _repositorySyntheticPopulation.DeleteAllAsync();
                await _repositoryPopulation.DeleteAllAsync();
                var importerService = _serviceProvider.GetRequiredService<IDataImporterService>();
                await importerService.InitializeFADNCodes();

                return Ok("All entities have been removed from database");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// This endpoint returns a 200 response when queried.         
        /// </summary>
        /// <returns></returns>
        [HttpGet("/ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok("Ping");
        }
    }
}



