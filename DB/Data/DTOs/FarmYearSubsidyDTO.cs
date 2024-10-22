using DB.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents farm year subsidy data.
    /// </summary>
    public class FarmYearSubsidyDTO
    {
        /// <summary>
        /// Gets or sets the farm ID.
        /// </summary>
        public long FarmId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Gets or sets the policy identifier.
        /// </summary>
        public string PolicyIdentifier { get; set; }
    }
}
