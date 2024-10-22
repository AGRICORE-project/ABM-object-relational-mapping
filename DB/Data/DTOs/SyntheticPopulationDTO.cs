using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a data transfer object (DTO) for a synthetic population,
    /// including its description, name, year, and population details.
    /// </summary>
    public class SyntheticPopulationJsonDTO
    {
        /// <summary>
        /// Gets or sets the description of the synthetic population.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the synthetic population.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the year number associated with the synthetic population.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the population details in the synthetic population.
        /// </summary>
        public PopulationJsonDTO Population { get; set; } = new PopulationJsonDTO();
    }
}
