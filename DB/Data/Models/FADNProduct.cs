using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Models
{
    [Index(nameof(FADNIdentifier), IsUnique = true)]
    /// <summary>
    /// Represents an FADN product with a unique identifier, description, product type, and relations.
    /// </summary>
    public class FADNProduct : Entity
    {
        /// <summary>
        /// The unique identifier for the FADN product.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string? FADNIdentifier { get; set; }

        /// <summary>
        /// The description of the FADN product.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public string? Description { get; set; }

        /*[Obsolete]
        public long? FADNGroupId { get; set; }
        [Obsolete]
        public FADNGroup? FADNGroup { get; set; }*/

        /// <summary>
        /// The type of product (Agricultural or Livestock).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [Required]
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Indicates if the product is arable.
        /// </summary>
        public bool Arable { get; set; }

        /// <summary>
        /// A list of FADN product relations.
        /// </summary>
        public List<FADNProductRelation>? FADNProductRelations { get; set; }
    }

    /// <summary>
    /// Enumeration for different types of products.
    /// </summary>
    public enum ProductType
    {
        Agricultural = 0,
        Livestock = 1
    }
}
