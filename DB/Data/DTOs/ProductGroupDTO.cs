using DB.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DB.Data.DTOs
{
    /// <summary>
    /// Represents a JSON data transfer object (DTO) for a product group,
    /// including details about products, their types, and categories.
    /// </summary>
    public class ProductGroupJsonDTO
    {
        /// <summary>
        /// Gets or sets the name of the product group.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the product type associated with the group.
        /// </summary>
        public string? ProductType { get; set; }

        /// <summary>
        /// Gets or sets the original name of the product group in the data source.
        /// </summary>
        public string? OriginalNameDatasource { get; set; }

        /// <summary>
        /// Gets or sets the products included in the original dataset.
        /// </summary>
        public string? ProductsIncludedInOriginalDataset { get; set; }

        /// <summary>
        /// Gets or sets the organic production type for the product group.
        /// </summary>
        public OrganicProductionType Organic { get; set; } = OrganicProductionType.Undetermined;

        /// <summary>
        /// Gets or sets the list of FADN products associated with the product group.
        /// </summary>
        public List<FADNProductJsonDTO> FADNProducts { get; set; } = new List<FADNProductJsonDTO>();

        /// <summary>
        /// Gets or sets the model-specific categories associated with the product group.
        /// </summary>
        public string[] ModelSpecificCategories { get; set; } = Array.Empty<string>();
    }
}
