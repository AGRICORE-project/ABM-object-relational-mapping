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
    /// Represents greening farm year data creation.
    /// </summary>
    public class GreeningFarmYearDataCreateDTO
    {
        /// <summary>
        /// Gets or sets the year ID.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the greening surface.
        /// </summary>
        public float GreeningSurface { get; set; }
    }

    /// <summary>
    /// Represents greening farm year data.
    /// </summary>
    public class GreeningFarmYearDataJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the greening surface.
        /// </summary>
        public float GreeningSurface { get; set; }
    }
    
}
