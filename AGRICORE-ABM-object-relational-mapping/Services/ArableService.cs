using CsvHelper.Configuration;
using CsvHelper;
using DB.Data.Models;
using DB.Data.Repositories;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Services
{
    public interface IArableService
    {
        /// <summary>
        /// Updates the arable condition of a product group.
        /// </summary>
        /// <param name="productGroupId">ID of the product group to update.</param>
        /// <returns>A boolean indicating if the update was successful.</returns>
        Task<bool> UpdateProductGroupArableCondition(long productGroupId);
    }

    /// <summary>
    /// Service implementation for managing arable conditions of product groups.
    /// </summary>
    public class ArableService:IArableService
    {
        private readonly IRepository<ProductGroup> _productGroupRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArableService"/> class.
        /// </summary>
        /// <param name="productGroupRepository">Repository for product groups.</param>

        public ArableService (IRepository<ProductGroup> productGroupRepository)
        {
            _productGroupRepository = productGroupRepository;
        }

        /// <summary>
        /// Updates the arable condition of a product group.
        /// </summary>
        /// <param name="productGroupId">ID of the product group to update.</param>
        /// <returns>A boolean indicating if the update was successful.</returns>

        public async Task<bool> UpdateProductGroupArableCondition(long productGroupId)
        {
            var group = await _productGroupRepository.GetSingleOrDefaultAsync(g => g.Id == productGroupId, include: g => g.Include(g => g.FADNProductRelations).ThenInclude(r => r.FADNProduct));

            if (group == null)
                return false;

            if (group.ModelSpecificCategories == null)
                group.ModelSpecificCategories = Array.Empty<string>();

            if (group?.FADNProductRelations == null)
            {
                if (group.ModelSpecificCategories.Any(q => String.Equals("Arable", q)))
                {
                    group.ModelSpecificCategories.Except(new string[] { "Arable" }).Order();
                }
                return _productGroupRepository.Update(group).Item1; 
            }

            int arables = 0;

            foreach(FADNProductRelation relation in group.FADNProductRelations) {

                if (relation.FADNProduct == null)
                    return false;

                if(relation.FADNProduct.Arable)
                    arables++;
            }

            if (arables > group.FADNProductRelations.Count / 2)
            {
                if (!group.ModelSpecificCategories.Any(q => String.Equals("Arable", q)))
                {
                    group.ModelSpecificCategories.Append("Arable").Order();
                }
            }
            else
            {
                if (group.ModelSpecificCategories.Any(q => String.Equals("Arable", q)))
                {
                    group.ModelSpecificCategories.Except(new string[] { "Arable" }).Order();
                }
            }
                

            _productGroupRepository.Update(group);

            return true;

        }

    }

    
}
