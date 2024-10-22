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
    /// Represents holder farm year data creation.
    /// </summary>
    public class HolderFarmYearDataCreateDTO
    {
        /// <summary>
        /// Gets or sets the year ID.
        /// </summary>
        public long YearId { get; set; }

        /// <summary>
        /// Gets or sets the holder's age.
        /// </summary>
        public int HolderAge { get; set; }

        /// <summary>
        /// Gets or sets the number of holder family members.
        /// </summary>
        public int HolderFamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the age of holder's successors.
        /// </summary>
        public int HolderSuccessorsAge { get; set; }

        /// <summary>
        /// Gets or sets the holder's gender.
        /// </summary>
        public int HolderGender { get; set; }

        /// <summary>
        /// Gets or sets the number of holder's successors.
        /// </summary>
        public long HolderSuccessors { get; set; }
    }

    /// <summary>
    /// Represents holder farm year data.
    /// </summary>
    public class HolderFarmYearDataJsonDTO
    {
        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        public long YearNumber { get; set; }

        /// <summary>
        /// Gets or sets the holder's age.
        /// </summary>
        public int HolderAge { get; set; }

        /// <summary>
        /// Gets or sets the number of holder family members.
        /// </summary>
        public int HolderFamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the age of holder's successors.
        /// </summary>
        public int HolderSuccessorsAge { get; set; }

        /// <summary>
        /// Gets or sets the holder's gender.
        /// </summary>
        public string HolderGender { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of holder's successors.
        /// </summary>
        public long HolderSuccessors { get; set; }
    }
    
}
