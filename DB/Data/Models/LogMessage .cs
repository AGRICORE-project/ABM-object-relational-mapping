using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a log message for a simulation run.
    /// </summary>
    public class LogMessage : Entity
    {
        /// <summary>
        /// The ID of the simulation run.
        /// </summary>
        public long SimulationRunId { get; set; }

        /// <summary>
        /// The simulation run entity.
        /// </summary>
        [JsonIgnore]
        public SimulationRun? SimulationRun { get; set; } = null!;

        /// <summary>
        /// The timestamp of the log message.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public long TimeStamp { get; set; }

        /// <summary>
        /// The source of the log message.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// The log level of the message.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The title of the log message.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The description of the log message.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        [NotNull]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enumeration for different levels of log messages.
    /// </summary>
    public enum LogLevel
    {
        TRACE = 5,
        DEBUG = 10,
        INFO = 20,
        SUCCESS = 25,
        WARNING = 30,
        ERROR = 40,
        CRITICAL = 50,
    }
}
