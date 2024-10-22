using CsvHelper.Configuration;
using CsvHelper;
using DB.Data.Models;
using DB.Data.Repositories;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using AutoMapper;
using DB.Data.DTOs;

namespace AGRICORE_ABM_object_relational_mapping.Services
{
    /// <summary>
    /// Service for running simulation scenarios.
    /// </summary>
    public interface ISimulationTasksService
    {
        Task<bool> RunSimulationScenario(SimulationScenario scenario, string queueSuffix = "");
    }

    public class SimulationTasksService : ISimulationTasksService
    {
        private readonly string _endpoint;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for SimulationTasksService.
        /// </summary>
        /// <param name="mapper">Mapper instance for mapping objects.</param>
        /// <exception cref="Exception">Thrown if SIMULATION_MANAGER_BASE environment variable is not set.</exception>

        public SimulationTasksService(IMapper mapper)
        {
            var value = Environment.GetEnvironmentVariable("SIMULATION_MANAGER_BASE");
            if (value == null)
                throw new Exception("SIMULATION_MANAGER_BASE environment variable not set");
            else
                _endpoint = value.TrimEnd('/') + "/tasks/simulationScenario/";
            _mapper = mapper;
        }

        /// <summary>
        /// Runs a simulation scenario by sending it to a simulation manager endpoint.
        /// </summary>
        /// <param name="scenario">SimulationScenario object representing the scenario to run.</param>
        /// <param name="queueSuffix">Optional suffix for the queue name.</param>
        /// <returns>True if the simulation scenario was successfully sent; otherwise, false.</returns>

        public async Task<bool> RunSimulationScenario(SimulationScenario scenario, string queueSuffix = "")
        {
            if (scenario == null)
                return false;
            else
            {
                var client = new HttpClient();
                var data = _mapper.Map<SimulationScenarioWithIdDTO>(scenario);
                data.QueueSuffix = queueSuffix;

                var result = await client.PostAsJsonAsync(_endpoint, data);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
