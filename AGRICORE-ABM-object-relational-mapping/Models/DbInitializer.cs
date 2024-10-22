// ***********************************************************************
// Assembly         : Internal Tools
// Author           : Jose
// Created          : 02-18-2021
//
// Last Modified By : Jose
// Last Modified On : 01-31-2022
// ***********************************************************************
// <copyright file="DbInitializer.cs" company="Internal Tools">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading;
using DB.Data.Models;
using DB.Data.Repositories;
using AGRICORE_ABM_object_relational_mapping.Services;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System.Xml.Linq;
using AGRICORE_ABM_object_relational_mapping.Controllers;

namespace DB.Data
{
    /// <summary>
    /// Class DbInitializer.
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static async void Initialize(IServiceProvider serviceProvider)
        {
            //DI
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AgricoreContext>>();
            var importerService = serviceProvider.GetRequiredService<IDataImporterService>();
            var yearRepository = serviceProvider.GetRequiredService<IRepository<Year>>();
            var populationRepository = serviceProvider.GetRequiredService<IRepository<Population>>();
            var simulationScenarioRepository = serviceProvider.GetRequiredService<IRepository<SimulationScenario>>();
            var simulationRunRepository = serviceProvider.GetRequiredService<IRepository<SimulationRun>>();
            var logMessageRepository = serviceProvider.GetRequiredService<IRepository<LogMessage>>();
            var syntheticRepository = serviceProvider.GetRequiredService<IRepository<SyntheticPopulation>>();
            var productGroupRepository = serviceProvider.GetRequiredService<IRepository<ProductGroup>>();
            var agriculturalProductionRepository = serviceProvider.GetRequiredService<IRepository<AgriculturalProduction>>();
            var livestockProductionRepository = serviceProvider.GetRequiredService<IRepository<LivestockProduction>>();
            var policyRepository = serviceProvider.GetRequiredService<IRepository<Policy>>();
            var holderRepository = serviceProvider.GetRequiredService<IRepository<HolderFarmYearData>>();
            var policyGroupRelationRepository = serviceProvider.GetRequiredService<IRepository<PolicyGroupRelation>>();
            var fadnProductRelationRepository = serviceProvider.GetRequiredService<IRepository<FADNProductRelation>>();
            var fadnProductRepository = serviceProvider.GetRequiredService<IRepository<FADNProduct>>();
            var closingFarmValuesRepository = serviceProvider.GetRequiredService<IRepository<ClosingValFarmValue>>();
            var farmYearSubsidieRepository = serviceProvider.GetRequiredService<IRepository<FarmYearSubsidy>>();
            var arableService = serviceProvider.GetRequiredService<IArableService>();
            var logger = serviceProvider.GetRequiredService<ILogger<DbInitializer>>();
            //Migrations
            var isConnected = false;
            using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();

            while (isConnected == false)
            {
                try
                {
                    applicationDbContext.Database.Migrate();
                    isConnected = true;
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception: " + ex.ToString());
                }
                Thread.Sleep(1_000);
            }

            //DI
            var webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            var _logger = serviceProvider.GetRequiredService<ILogger<DbInitializer>>();

            //Seed data

            await importerService.InitializeFADNCodes();

            //if (!applicationDbContext.FADNProducts.Any()) {
            //    await importerService.InitializeFADNCodes();
            //}

            if (webHostEnvironment.IsProduction())
            {
                /* 
                 * Please note: Only uncomment methods when the seeding is needed and comment them back 
                 * when the production seeding is complete to avoid extra code execution and for safety reasons.
                 * Remember to also uncomment the last line: applicationDbContext.SaveChanges();
                 */

                //applicationDbContext.SaveChanges();
            }

            if (!webHostEnvironment.IsDevelopment())
            {
                applicationDbContext.SaveChanges();
                return;
            }

            #region Development seed data

            bool seedData = false;

            if (seedData) {
                var exisitingPopulations = await populationRepository.GetAllAsync();

                if (exisitingPopulations.Count == 0)
                {
                    await CreatePopulation(populationRepository, _logger);
                    await CreateYears(yearRepository, populationRepository, _logger);
                    await CreateSimulations(simulationScenarioRepository, simulationRunRepository, logMessageRepository, populationRepository, _logger);
                    await CreateSyntheticPopulation(syntheticRepository, populationRepository, _logger);
                    await CreateProductGroups(productGroupRepository, populationRepository, _logger);
                    await CreateProductions(agriculturalProductionRepository, livestockProductionRepository, populationRepository, _logger);
                    await CreatePolicies(policyRepository, _logger);
                    await CreateHolders(populationRepository, holderRepository, _logger);
                    await CreatePolicyGroupRelations(populationRepository, policyRepository, policyGroupRelationRepository, _logger);
                    await CreateFadnProductRelations(populationRepository, fadnProductRepository, fadnProductRelationRepository, _logger, arableService);
                    await CreateClosingFarmValues(populationRepository, closingFarmValuesRepository, _logger);
                    await CreateFarmYearSubsidies(populationRepository, farmYearSubsidieRepository, _logger);
                }

                applicationDbContext.SaveChanges();
            }
            #endregion Development seed data
        }

        /// <summary>
        /// Creates farm year subsidies for all populations.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="farmYearSubsidieRepository">Repository for accessing farm year subsidies.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateFarmYearSubsidies(IRepository<Population> populationRepository, IRepository<FarmYearSubsidy> farmYearSubsidieRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Farms)
                .Include(p => p.Years)
                .Include(p => p.ProductGroups)
                .ThenInclude(pg => pg.PolicyGroupRelations)
                .ThenInclude(pgr => pgr.Policy));

            Random random = new Random();
            List<FarmYearSubsidy> farmYearSubsidies = new List<FarmYearSubsidy>();

            foreach (var population in populations) {

                List<Policy> policies = new List<Policy>();
                foreach(var productGroup in population.ProductGroups)
                {
                    foreach(var policyGroupRelation in productGroup.PolicyGroupRelations)
                    {
                        policies.Add(policyGroupRelation.Policy);
                    }
                }

                foreach(var farm in population.Farms)
                {
                    foreach(var year in population.Years)
                    {
                        for(int i = 0; i < random.Next(5, 10); i++)
                        {
                            FarmYearSubsidy farmYearSubsidy = new FarmYearSubsidy
                            {
                                FarmId = farm.Id,
                                YearId = year.Id,
                                PolicyId = policies[random.Next(policies.Count)].Id,
                                Value = random.Next(10,500)
                            };

                            var exist = await farmYearSubsidieRepository.GetSingleOrDefaultAsync(fys => fys.FarmId == farm.Id && fys.YearId == year.Id && fys.PolicyId == farmYearSubsidy.PolicyId);
                            if (exist != null || farmYearSubsidies.Any(fys => fys.FarmId == farm.Id && fys.YearId == year.Id && fys.PolicyId == farmYearSubsidy.PolicyId))
                                continue;
                            farmYearSubsidies.Add(farmYearSubsidy);
                        }
                    }
                }
            }
            var result = await farmYearSubsidieRepository.AddRangeAsync(farmYearSubsidies);
            if (!result.Item1)
            {
                logger.LogError("Could not create default farm year subsidies");
            }
        }

        /// <summary>
        /// Creates closing farm values for all populations.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="closingFarmValuesRepository">Repository for accessing closing farm values.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateClosingFarmValues(IRepository<Population> populationRepository, IRepository<ClosingValFarmValue> closingFarmValuesRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Farms).Include(p => p.Years));
            List<ClosingValFarmValue> closingValFarmValues = new List<ClosingValFarmValue>();
            foreach (var population in populations)
            {
                foreach(var farm in population.Farms)
                {
                    foreach(var year in population.Years)
                    {
                        ClosingValFarmValue closingValFarmValue = new ClosingValFarmValue
                        {
                            FarmId = farm.Id,
                            YearId = year.Id,
                        };
                        closingValFarmValues.Add(closingValFarmValue);
                    }
                }
            }
            var result = await closingFarmValuesRepository.AddRangeAsync(closingValFarmValues);
            if (!result.Item1)
            {
                logger.LogError("Could not create default closing val farm values");
            }
        }

        /// <summary>
        /// Creates FADN product relations for all populations.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="fadnProductRepository">Repository for accessing FADN products.</param>
        /// <param name="fadnProductRelationRepository">Repository for accessing FADN product relations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <param name="arableService">Service for updating arable conditions.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateFadnProductRelations(IRepository<Population> populationRepository, IRepository<FADNProduct> fadnProductRepository, IRepository<FADNProductRelation> fadnProductRelationRepository, ILogger<DbInitializer> logger, IArableService arableService)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.ProductGroups));
            var fadnproducts = await fadnProductRepository.GetAllAsync();
            Random random = new Random();
            List<FADNProductRelation> fADNProductRelations = new List<FADNProductRelation>();
            foreach(var population in populations)
            {
                foreach(var productgroup in population.ProductGroups)
                {
                    for(int i = 0; i< random.Next(10, fadnproducts.Count/2); i++)
                    {
                        FADNProductRelation fADNProductRelation = new FADNProductRelation
                        {
                            PopulationId = population.Id,
                            ProductGroupId = productgroup.Id,
                            FADNProductId = fadnproducts[random.Next(fadnproducts.Count)].Id
                        };

                        var exist = await fadnProductRelationRepository.GetSingleOrDefaultAsync(pg => pg.PopulationId == population.Id && pg.ProductGroupId == productgroup.Id && pg.FADNProductId == fADNProductRelation.FADNProductId);
                        if (exist != null || fADNProductRelations.Any(pg => pg.PopulationId == population.Id && pg.ProductGroupId == productgroup.Id && pg.FADNProductId == fADNProductRelation.FADNProductId))
                            continue;
                        fADNProductRelations.Add(fADNProductRelation);
                    }
                }
            }

            var result = await fadnProductRelationRepository.AddRangeAsync(fADNProductRelations);
            if (!result.Item1)
            {
                logger.LogError("Could not create default fadn group relations");
            }
            // Disabled to leave arable condition to be manually updated in the import process
            //foreach (var population in populations)
            //{
            //    foreach(var productgroup in population.ProductGroups)
            //    {
            //        await arableService.UpdateProductGroupArableCondition(productgroup.Id);
            //    }
            //}
        }

        /// <summary>
        /// Creates policy group relations for all populations.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="policyRepository">Repository for accessing policies.</param>
        /// <param name="policyGroupRelationRepository">Repository for accessing policy group relations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreatePolicyGroupRelations(IRepository<Population> populationRepository, IRepository<Policy> policyRepository, IRepository<PolicyGroupRelation> policyGroupRelationRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.ProductGroups));
            var policies = await policyRepository.GetAllAsync();
            Random random = new Random();
            List<PolicyGroupRelation> policyGroupRelations = new List<PolicyGroupRelation>();

            foreach(var population in populations) { 
                foreach(var productgroup in population.ProductGroups)
                {
                    for (int i = 0; i < random.Next(policies.Count); i++)
                    {

                        PolicyGroupRelation policyGroupRelation = new PolicyGroupRelation
                        {
                           PopulationId = population.Id,
                           ProductGroupId = productgroup.Id,
                           PolicyId = policies[random.Next(policies.Count)].Id
                        };

                        var exist = await policyGroupRelationRepository.GetSingleOrDefaultAsync(pg => pg.PopulationId == population.Id && pg.ProductGroupId == productgroup.Id && pg.PolicyId == policyGroupRelation.PolicyId);
                        if (exist != null || policyGroupRelations.Any(pg => pg.PopulationId == population.Id && pg.ProductGroupId == productgroup.Id && pg.PolicyId == policyGroupRelation.PolicyId))
                            continue;

                        policyGroupRelations.Add(policyGroupRelation);
                    }
                }
            }


            var result = await policyGroupRelationRepository.AddRangeAsync(policyGroupRelations);
            if (!result.Item1)
            {
                logger.LogError("Could not create default policy group relations");
            }
        }

        /// <summary>
        /// Creates holder data for all farms and years in all populations.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="holderRepository">Repository for accessing holder farm year data.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateHolders(IRepository<Population> populationRepository, IRepository<HolderFarmYearData> holderRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Years).Include(p => p.Farms));
            Random random = new Random();
            var gendervalues = Enum.GetValues(typeof(GenderEnum));
            List<HolderFarmYearData> holderFarmYearDatas = new List<HolderFarmYearData>();
            foreach(var population in populations)
            {
                foreach(var farm in population.Farms)
                {
                    foreach(var year in population.Years)
                    {
                        HolderFarmYearData holder = new HolderFarmYearData
                        {
                            FarmId = farm.Id,
                            YearId = year.Id,
                            HolderAge = random.Next(20, 50),
                            HolderFamilyMembers = random.Next(1, 5),
                            HolderSuccessors = 1,
                            HolderSuccessorsAge = random.Next(20),
                            HolderGender = (GenderEnum)gendervalues.GetValue(random.Next(gendervalues.Length)),
                        };

                        holderFarmYearDatas.Add(holder);
                    }
                }
            }

            var result = await holderRepository.AddRangeAsync(holderFarmYearDatas);
            if (!result.Item1)
            {
                logger.LogError("Could not create default holders");
            }
        }

        /// <summary>
        /// Creates policies with random data.
        /// </summary>
        /// <param name="policyRepository">Repository for accessing policies.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreatePolicies(IRepository<Policy> policyRepository, ILogger<DbInitializer> logger)
        {
            Random random = new Random();
            List<Policy> policies = new List<Policy>();
            for(int i = 0; i< random.Next(10,20); i++)
            {
                Policy policy = new Policy
                {
                    PolicyIdentifier = "policy" + i,
                    IsCoupled = random.Next(2) == 0 ? false : true,
                    PolicyDescription = "autogenerated"
                };
                policies.Add(policy);
            }

            var result = await policyRepository.AddRangeAsync(policies);
            if (!result.Item1)
            {
                logger.LogError("Could not create default policies");
            }
        }

        /// <summary>
        /// Creates agricultural and livestock productions for all populations.
        /// </summary>
        /// <param name="agriculturalProductionRepository">Repository for accessing agricultural productions.</param>
        /// <param name="livestockProductionRepository">Repository for accessing livestock productions.</param>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateProductions(IRepository<AgriculturalProduction> agriculturalProductionRepository, IRepository<LivestockProduction> livestockProductionRepository, IRepository<Population> populationRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Years).Include(p => p.Farms).Include(p => p.ProductGroups));
            Random random = new Random();

            var enumOrganicValues = Enum.GetValues(typeof(OrganicProductionType));
            List<AgriculturalProduction> agriculturalProductions = new List<AgriculturalProduction>();
            List<LivestockProduction> livestockProductions = new List<LivestockProduction>();

            foreach (var population  in populations)
            {
                var agriculturalGroups = population.ProductGroups.FindAll(pg => pg.ProductType == ProductType.Agricultural);
                var livestockGroups = population.ProductGroups.FindAll(pg => pg.ProductType == ProductType.Livestock);

                foreach( var farm in population.Farms)
                {
                    foreach(var year in population.Years)
                    {
                        for(int i = 0; i < random.Next(5,11); i++)
                        {
                            AgriculturalProduction agriculturalProduction = new AgriculturalProduction
                            {
                                YearId = year.Id,
                                FarmId = farm.Id,
                                OrganicProductionType = (OrganicProductionType)enumOrganicValues.GetValue(random.Next(enumOrganicValues.Length)),
                                ProductGroupId = agriculturalGroups[random.Next(agriculturalGroups.Count)].Id,
                                CropProduction = random.Next(500, 1000),
                                CultivatedArea = random.Next(50, 200),
                                IrrigatedArea = random.Next(50, 100),
                                LandValue = random.Next(1000, 2000),
                                QuantitySold = random.Next(100, 500),
                                SellingPrice = random.Next(50, 100),
                                ValueSales = random.Next(500, 1000),
                                VariableCosts = random.Next(200, 500),
                            };
                            if(!agriculturalProductions.Any(ap => ap.YearId == agriculturalProduction.YearId && ap.ProductGroupId == agriculturalProduction.ProductGroupId && ap.FarmId == agriculturalProduction.FarmId))
                                agriculturalProductions.Add(agriculturalProduction);

                            LivestockProduction livestockProduction = new LivestockProduction
                            {
                                YearId = year.Id,
                                FarmId = farm.Id,
                                ProductGroupId = livestockGroups[random.Next(livestockGroups.Count)].Id,
                                DairyCows = random.Next(50, 100),
                                EggsTotalProduction = random.Next(70, 100),
                                EggsProductionSold = random.Next(20, 70),
                                EggsTotalSales = random.Next(50, 100),
                                ManureTotalSales = random.Next(50, 100),
                                VariableCosts = random.Next(100, 500),
                                SellingPrice = random.Next(50, 100),
                                MilkTotalProduction = random.Next(70, 100),
                                MilkProductionSold = random.Next(20, 70),
                                MilkTotalSales = random.Next(50, 100),
                                MilkVariableCosts = random.Next(30, 50),
                                NumberAnimalsForSlaughtering = random.Next(20, 50),
                                NumberOfAnimals = random.Next(100, 150),
                                NumberAnimalsRearingBreading = random.Next(20, 50),
                                NumberOfAnimalsSold = random.Next(20, 50),
                                ValueAnimalsRearingBreading = random.Next(20, 50),
                                ValueSlaughteredAnimals = random.Next(20, 50),
                                WoolTotalProduction = random.Next(50, 100),
                                WoolProductionSold = random.Next(20, 50),
                                ValueSoldAnimals = random.Next(50, 100)
                            };
                            if(!livestockProductions.Any(lp => lp.YearId == livestockProduction.YearId && lp.ProductGroupId == livestockProduction.ProductGroupId && lp.FarmId == livestockProduction.FarmId))
                                livestockProductions.Add(livestockProduction);
                        }
                    }
                }
            }

            var result = await agriculturalProductionRepository.AddRangeAsync(agriculturalProductions);
            if (!result.Item1)
            {
                logger.LogError("Could not create default agricultural productions");
            }


            result = await livestockProductionRepository.AddRangeAsync(livestockProductions);
            if (!result.Item1)
            {
                logger.LogError("Could not create default livestock productions");
            }

        }

        /// <summary>
        /// Creates product groups for all populations.
        /// </summary>
        /// <param name="productGroupRepository">Repository for accessing product groups.</param>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateProductGroups(IRepository<ProductGroup> productGroupRepository, IRepository<Population> populationRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync();

            Random random = new Random();
            var enumvalues = Enum.GetValues(typeof(ProductType));
            List<ProductGroup> productGroups = new List<ProductGroup>();
            foreach (var population in populations) { 
            
                for(int i = 0; i < random.Next(5, 15); i++)
                {
                    var name = "pg-" + i + "-population" + population.Id;
                    ProductGroup productGroup = new ProductGroup(name,(ProductType)enumvalues.GetValue(random.Next(enumvalues.Length)),"none");
                    productGroup.PopulationId = population.Id;
                    productGroup.ModelSpecificCategories = new string[] { };
                    productGroups.Add(productGroup);
                }
            }

            var result = await productGroupRepository.AddRangeAsync(productGroups);
            if (!result.Item1)
            {
                logger.LogError("Could not create default product groups");
            }
        }

        /// <summary>
        /// Creates synthetic populations if none exist.
        /// </summary>
        /// <param name="syntheticRepository">Repository for accessing synthetic populations.</param>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateSyntheticPopulation(IRepository<SyntheticPopulation> syntheticRepository, IRepository<Population> populationRepository, ILogger<DbInitializer> logger)
        {
            var synthetic = await syntheticRepository.GetAllAsync();
            List<SyntheticPopulation> syntheticPopulations = new List<SyntheticPopulation>();

            if(synthetic == null || synthetic.Count == 0)
            {
                var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Years));

                foreach (var population in populations)
                {
                    foreach(var year in population.Years)
                    {
                        SyntheticPopulation syntheticPopulation = new SyntheticPopulation
                        {
                            PopulationId = population.Id,
                            YearId = year.Id,
                            Description = "",
                            Name = ""
                        };

                        syntheticPopulations.Add(syntheticPopulation);
                    }
                }

                var result = await syntheticRepository.AddRangeAsync(syntheticPopulations);
                if (!result.Item1)
                {
                    logger.LogError("Could not create default synthetic populations");
                }
            }

        }

        /// <summary>
        /// Creates simulation scenarios for all populations.
        /// </summary>
        /// <param name="simulationScenarioRepository">Repository for accessing simulation scenarios.</param>
        /// <param name="simulationRunRepository">Repository for accessing simulation runs.</param>
        /// <param name="logMessageRepository">Repository for accessing log messages.</param>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateSimulations(IRepository<SimulationScenario> simulationScenarioRepository, IRepository<SimulationRun> simulationRunRepository, IRepository<LogMessage> logMessageRepository, IRepository<Population> populationRepository, ILogger<DbInitializer> logger)
        {
            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Years));
            List<SimulationScenario> simulationScenarios = new List<SimulationScenario>();

            foreach (var population in populations)
            {
                foreach(var year in population.Years)
                {
                    SimulationScenario simulationScenario = new SimulationScenario
                    {
                        PopulationId = population.Id,
                        YearId = year.Id,
                        SimulationRun = null,
                        ShortTermModelBranch = "",
                        LongTermModelBranch = "",
                        Horizon = 0,
                    };

                    simulationScenario.SimulationRun = new SimulationRun
                    {
                        LogMessages = new List<LogMessage>(),
                        OverallStatus = OverallStatus.INPROGRESS,
                        CurrentStage = SimulationStage.DATAPREPARATION,
                        CurrentYear = 0,
                        CurrentSubstage = "",
                        CurrentStageProgress = 0,
                        CurrentSubStageProgress = 0,
                    };

                    for (int i = 0; i < 5; i++)
                    {
                        simulationScenario.SimulationRun.LogMessages.Add(new LogMessage
                        {
                            Description = "",
                            LogLevel = Models.LogLevel.TRACE,
                            Source = "",
                            TimeStamp = 0,
                            Title = ""
                        });
                    }

                    simulationScenarios.Add(simulationScenario);
                }
                
            }

            var result = await simulationScenarioRepository.AddRangeAsync(simulationScenarios);
            if (!result.Item1)
            {
                logger.LogError("Could not create default simulation scenarios");
            }

        }

        /// <summary>
        /// Creates years for all populations if they do not already exist.
        /// </summary>
        /// <param name="yearRepository">Repository for accessing years.</param>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreateYears(IRepository<Year> yearRepository, IRepository<Population> populationRepository, ILogger<DbInitializer> logger)
        {
            int[] years = { 2019, 2020, 2021, 2022 };
            List<Year> yearList = new List<Year>();

            var populations = await populationRepository.GetAllAsync(include: p => p.Include(p => p.Years));

            foreach (int year in years)
            {
                foreach(var population in populations)
                {
                    bool existingYear = population.Years.Any(y => y.YearNumber == year);
                    if (!existingYear)
                    {
                            Year ty = new Year { YearNumber = year, PopulationId = population.Id };
                            yearList.Add(ty);
                    }
                };
            }
            if(yearList.Count > 0)
            {
                var result = await yearRepository.AddRangeAsync(yearList);
                if (!result.Item1)
                {
                    logger.LogError("Could not create default years");
                }
            }

        }

        /// <summary>
        /// Creates default populations with farms.
        /// </summary>
        /// <param name="populationRepository">Repository for accessing populations.</param>
        /// <param name="logger">Logger for logging errors.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private static async Task CreatePopulation(IRepository<Population> populationRepository,ILogger<DbInitializer> logger)
        {
            
            Random rnd = new Random();
                

            List<Population> populationList = new()
            {
                new Population()
                {
                    Description = "generated by dbInitializer: population 1"
                },
                new Population()
                {
                    Description = "generated by dbInitializer: population 2"
                }
            };
            foreach (var pop in populationList)
            {
                pop.Farms = new List<Farm>();
                for (int i = 0; i < rnd.Next(1,100); i++)
                {
                    pop.Farms.Add(new Farm
                    {
                        AgriculturalProductions = new List<AgriculturalProduction>(),
                        Altitude = 0,
                        FarmCode = "Farm_" + i.ToString(),
                        Lat = rnd.Next(100),
                        Long = rnd.Next(100),
                        LivestockProductions = new List<LivestockProduction>(),
                        RegionLevel1 = "0",
                        RegionLevel2 = "0",
                        RegionLevel3 = 0,
                        RegionLevel1Name = "",
                        RegionLevel2Name = "",
                        RegionLevel3Name = "",
                        TechnicalEconomicOrientation = 0,
                        HoldersFarmYearData = new List<HolderFarmYearData>()
                    });
                }
            }
            var result = await populationRepository.AddRangeAsync(populationList);
            if (!result.Item1)
            {
                logger.LogError("Could not create a default population");
            } 
            
        }
    }
}