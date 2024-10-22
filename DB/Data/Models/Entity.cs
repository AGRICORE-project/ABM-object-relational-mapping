using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace DB.Data.Models
{
    /// <summary>
    /// Base class for all entities, providing a unique identifier.
    /// </summary>
    public abstract class Entity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema(ReadOnly = true)]
        public virtual long Id { get; set; }
    }
}
