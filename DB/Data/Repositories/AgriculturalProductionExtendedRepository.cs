using DB.Data.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data.Repositories
{
    /// <summary>
    /// Provides extended repository operations for <see cref="AgriculturalProduction"/> entities.
    /// </summary>
    public class AgriculturalProductionExtendedRepository : Repository<AgriculturalProduction>, IAgriculturalProductionExtendedRepository
    {
        private readonly IDbContextFactory<AgricoreContext> _dbContextFactory;
        private readonly ILogger<Repository<AgriculturalProduction>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgriculturalProductionExtendedRepository"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The database context factory.</param>
        /// <param name="logger">The logger.</param>
        public AgriculturalProductionExtendedRepository(IDbContextFactory<AgricoreContext> dbContextFactory, ILogger<Repository<AgriculturalProduction>> logger) : base(dbContextFactory, logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets a dictionary of average variable costs for each product group in a specific population.
        /// </summary>
        /// <param name="populationId">The identifier of the population.</param>
        /// <returns>A dictionary where the key is the product group identifier and the value is the average variable costs.</returns>
        public async Task<Dictionary<long, float>> GetRegionDictionaryOfVariableCosts(long populationId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = await context.AgriculturalProductions
                .Where(ap => ap.Farm.PopulationId == populationId && ap.VariableCosts > 0)
                .Include(q => q.Farm)
                .Select(q => new { ProductGroupId = q.ProductGroupId.Value, VariableCosts = q.VariableCosts.Value })
                .GroupBy(q => q.ProductGroupId)
                .AsNoTracking()
                .ToDictionaryAsync(g => g.Key, g => g.Average(q => q.VariableCosts));

            await context.DisposeAsync();

            return query;
        }
    }

    /// <summary>
    /// Defines additional operations for the <see cref="AgriculturalProduction"/> repository.
    /// </summary>
    public interface IAgriculturalProductionExtendedRepository : IRepository<AgriculturalProduction>
    {
        /// <summary>
        /// Gets a dictionary of average variable costs for each product group in a specific population.
        /// </summary>
        /// <param name="populationId">The identifier of the population.</param>
        /// <returns>A dictionary where the key is the product group identifier and the value is the average variable costs.</returns>
        Task<Dictionary<long, float>> GetRegionDictionaryOfVariableCosts(long populationId);
    }
}
