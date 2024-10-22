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
using System.Xml.Linq;

namespace DB.Data.Models
{
    /// <summary>
    /// Represents a group of products within a specific population.
    /// The product group is defined by its name, product type, and original data source, among other attributes.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Name), nameof(PopulationId), IsUnique = true)]
    public class ProductGroup : Entity
    {
        /// <summary>
        /// Gets or sets the name of the product group.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the list of FADN product relations associated with this product group.
        /// </summary>
        [JsonIgnore]
        public List<FADNProductRelation>? FADNProductRelations { get; set; }

        /// <summary>
        /// Gets or sets the list of policy group relations associated with this product group.
        /// </summary>
        [JsonIgnore]
        public List<PolicyGroupRelation>? PolicyGroupRelations { get; set; }

        /// <summary>
        /// Gets or sets the type of product (Agricultural or Livestock) for this product group.
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Gets or sets the original name of the datasource from which this product group was derived.
        /// </summary>
        public string? OriginalNameDatasource { get; set; }

        /// <summary>
        /// Gets or sets the products included in the original dataset for this product group.
        /// </summary>
        public string? ProductsIncludedInOriginalDataset { get; set; }

        /// <summary>
        /// Gets or sets the type of organic production for this product group. Default is Undetermined.
        /// </summary>
        public OrganicProductionType Organic { get; set; } = OrganicProductionType.Undetermined;

        /// <summary>
        /// Gets or sets the model-specific categories associated with this product group.
        /// </summary>
        public string[] ModelSpecificCategories { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the population identifier associated with this product group.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the population to which this product group belongs.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductGroup"/> class with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the product group.</param>
        /// <param name="productType">The type of product (Agricultural or Livestock) for this product group.</param>
        /// <param name="originalNameDatasource">The original name of the datasource from which this product group was derived.</param>
        public ProductGroup(string name, ProductType productType, string originalNameDatasource)
        {
            Name = name;
            OriginalNameDatasource = originalNameDatasource;
            ProductType = productType;
        }

        /// <summary>
        /// Determines whether this product group is categorized as "Other" based on its model-specific categories.
        /// </summary>
        /// <returns>True if the product group includes the category "Other"; otherwise, false.</returns>
        public bool IsOther()
        {
            if (this.ModelSpecificCategories != null && this.ModelSpecificCategories.Length > 0 && this.ModelSpecificCategories.Any(q => q.Equals("Other", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Represents the relation between a product group and a FADN product within a specific population.
    /// This relation includes information about the representativeness of the FADN product within the product group.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    [Index(nameof(ProductGroupId), nameof(FADNProductId), nameof(PopulationId), IsUnique = true)]
    public class FADNProductRelation : Entity
    {
        /// <summary>
        /// Gets or sets the identifier of the product group associated with this relation.
        /// </summary>
        public long ProductGroupId { get; set; }

        /// <summary>
        /// Gets or sets the product group associated with this relation.
        /// </summary>
        [JsonIgnore]
        public ProductGroup? ProductGroup { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the FADN product associated with this relation.
        /// </summary>
        public long FADNProductId { get; set; }

        /// <summary>
        /// Gets or sets the FADN product associated with this relation.
        /// </summary>
        [JsonIgnore]
        public FADNProduct? FADNProduct { get; set; } = null!;

        /// <summary>
        /// Gets or sets the population identifier associated with this relation.
        /// </summary>
        public long PopulationId { get; set; }

        /// <summary>
        /// Gets or sets the population to which this relation belongs.
        /// </summary>
        [JsonIgnore]
        public Population? Population { get; set; } = null!;

        /// <summary>
        /// Gets or sets the representativeness of the FADN product in the product group based on occurrence.
        /// </summary>
        // The representativeness of the FADN product in the product group. Item representativeness should be calculated
        // as the representativeness of the FADN product divided by the sum of the representativeness of all FADN products in the product group.
        // For its calculation, remember than whe obtaining data from the sample, the weight of such farm in the population should be taken into account
        public float RepresentativenessOcurrence { get; set; }

        /// <summary>
        /// Gets or sets the representativeness of the FADN product in the product group based on area.
        /// </summary>
        public float RepresentativenessArea { get; set; }

        /// <summary>
        /// Gets or sets the representativeness of the FADN product in the product group based on value.
        /// </summary>
        public float RepresentativenessValue { get; set; }
    }

}
