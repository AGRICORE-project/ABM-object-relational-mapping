using Microsoft.EntityFrameworkCore;
using DB.Data.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace DB.Data.Repositories

{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        private readonly IDbContextFactory<AgricoreContext> _dbContextFactory;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(IDbContextFactory<AgricoreContext> dbContextFactory, ILogger<Repository<T>> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = context.Set<T>();
            IQueryable<T> query = entities;
            int count = -1;
            if (predicate != null)
            {
                count = await query.CountAsync(predicate);
            } else
            {
                count = await query.CountAsync();
            }
            await context.DisposeAsync();
            return count;
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                            bool asNoTracking = true,
                                            bool asSeparateQuery = false,
                                            int? take = null,
                                            int? skip = null,
                                            bool debugQuery = false)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = context.Set<T>();

            IQueryable<T> query = entities;

            if (predicate != null) query = query.Where(predicate);

            if (asNoTracking) query = query.AsNoTracking();

            if (orderBy != null) query = orderBy(query);

            if (skip != null) query = query.Skip(skip.Value);
            if (take != null)
            {
                query = query.Take(take.Value);
            }

            if (include != null)
            {
                if (asSeparateQuery) query = query.AsSplitQuery();
                query = include(query);
            }

            if (debugQuery) 
                _logger.LogDebug(query.ToQueryString());

            var result = await query.ToListAsync();

            await context.DisposeAsync();

            return result;
        }

        public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>>? predicate = null,
                                                    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                                    bool asNoTracking = true,
                                                    bool asSeparateQuery = false)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = context.Set<T>();

            IQueryable<T> query = entities;

            if (asNoTracking) query = query.AsNoTracking();

            if (include != null)
            {
                if (asSeparateQuery) query = query.AsSplitQuery();
                query = include(query);
            }

            if (predicate != null) query = query.Where(predicate);

            var result = await query.SingleOrDefaultAsync();
            await context.DisposeAsync();

            return result;
        }
        /// <summary>
        /// Add an entity to a table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(bool, string?)> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using var context = await _dbContextFactory.CreateDbContextAsync();

            bool rowsChanged = false;
            string? errorMessage = null;
            try
            {
                await context.AddAsync(entity);
                rowsChanged = await context.SaveChangesAsync() > 0;
            }
            catch (Exception e) { errorMessage = e.Message; }
            finally { await context.DisposeAsync(); }

            return (rowsChanged, errorMessage);
        }
        /// <summary>
        /// Add a range of entities to a table
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(bool, string?)> AddRangeAsync(List<T> entities, int batchSize = 1000)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            var rowsChanged = false;
            string? errorMessage = null;

            using var context = await _dbContextFactory.CreateDbContextAsync();

            int nRowsChanged = 0;
            try
            {
                if (batchSize > 0)
                {
                    for (int i = 0; i < entities.Count; i += batchSize)
                    {
                        var batch = entities.Skip(i).Take(batchSize);
                        await context.AddRangeAsync(batch);
                        nRowsChanged = await context.SaveChangesAsync();
                        rowsChanged = nRowsChanged > 0;
                        if (!rowsChanged) break;
                    }
                }
                else
                {
                    await context.AddRangeAsync(entities);
                    nRowsChanged = await context.SaveChangesAsync();
                    rowsChanged = nRowsChanged > 0;
                }
            }
            catch (Exception e) { errorMessage = e.Message; }
            finally { await context.DisposeAsync(); }

            return (rowsChanged, errorMessage);
        }
        /// <summary>
        /// Update a certain entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public (bool, string?) Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using var context = _dbContextFactory.CreateDbContext();

            var rowsChanged = false;
            string? errorMessage = null;

            try
            {
                context.Update(entity);
                rowsChanged = context.SaveChanges() > 0;
            }
            catch (Exception e) { errorMessage = e.Message; }
            finally { context.Dispose(); }

            return (rowsChanged, errorMessage);
        }
        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public (bool, string?) Remove(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using var context = _dbContextFactory.CreateDbContext();

            bool rowsChanged = false;
            string? errorMessage = null;

            try
            {
                context.Remove(entity);
                rowsChanged = context.SaveChanges() > 0;
            }
            catch (Exception e) { errorMessage = e.Message; }
            finally { context.Dispose(); }

            return (rowsChanged, errorMessage);
        }
        /// <summary>
        /// Remove a range of entities
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public (bool, string?) RemoveRange(List<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            using var context = _dbContextFactory.CreateDbContext();

            bool rowsChanged = false;
            string? errorMessage = null;

            try
            {
                context.RemoveRange(entities);
                rowsChanged = context.SaveChanges() > 0;
            }
            catch (Exception e) { errorMessage = e.Message; }
            finally { context.Dispose(); }

            return (rowsChanged, errorMessage);
        }
        /// <summary>
        /// Delete all entities from a given repository
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, string?)> DeleteAllAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();

            string? errorMessage = null;

            try
            {
                var allEntities = await GetAllAsync(asNoTracking: false); // Retrieve all entities of type T
                context.RemoveRange(allEntities); // Remove all entities from the context
                await context.SaveChangesAsync(); // Save changes to delete the entities in the database
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }

            return (errorMessage == null, errorMessage);
        }
        /// <summary>
        /// Update a list of entities in an asynchronous way
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, string?)> UpdateRangeAsync(IEnumerable<T> entities)
        {
            using var context = _dbContextFactory.CreateDbContext();

            string? errorMessage = null;

            try
            {
                context.UpdateRange(entities); // Update all entities
                int result = await context.SaveChangesAsync(); // Save changes 
                if (result == 0) errorMessage = "No entities were updated";
                if (result != entities.Count()) errorMessage = "Not all entities were updated";
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            finally {
                await context.DisposeAsync();
            }

            return (errorMessage == null, errorMessage);
        }

    }

    public interface IRepository<T>
    {

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Gets all the entities of <see cref="IRepository{T}"></see> that match the given expression
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition (optional)</param>
        /// <param name="orderBy">An expression to be applied to order the entities (optional)</param>
        /// <param name="include">An expression to be applied to include child entities (optional)</param>
        /// <param name="asNoTracking">Boolean to use AsNoTracking when getting the entities. Defaults to true</param>
        /// <returns>The entity with the given id or null if it is not found></see></returns>
        /// <remarks>This method defaults to no tracking query</remarks>
        /// <example>
        /// Usage: 
        /// <code>
        /// // After injecting the repository into _operationLogRepository for data class OperationLog
        /// List<OperationLog> opLogs = await _operationLogRepository.GetAllAsync(predicate: op => op.Name == "Unnamed"
        ///                                                          , orderBy: op => op.OrderBy(op => op.StartDateTime)
        ///                                                          , include: op => op.Include(op => op.MachiningProcess)
        ///                                                          , asNoTracking: true);
        /// </code>
        /// </example>
        public Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true, bool asSeparateQuery = false, int? take = null, int? skip = null, bool debugQuery = false);

        /// <summary>
        /// Gets the single entity of <see cref="IRepository{T}"></see> tha matches the given predicate. Use this method to implement GetById.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition (optional)</param>
        /// <param name="include">An expression to be applied to include child entities (optional)</param>
        /// <param name="asNoTracking">Boolean to use AsNoTracking when getting the entities. Defaults to true</param>
        /// <returns>The entity with the given id or null if it is not found></see></returns>
        /// <remarks>This method defaults to no tracking query</remarks>
        /// <example>
        /// Usage: 
        /// <code>
        /// // After injecting the repository into _operationLogRepository for data class OperationLog
        /// List<OperationLog> opLogs = await _operationLogRepository.GetSingleOrDefaultAsync(predicate: op => op.Id == "847ccec3-3bea-43a6-997f-8497d6da3412"
        ///                                                          , include: op => op.Include(op => op.MachiningProcess)
        ///                                                          , asNoTracking: true);
        /// </code>
        /// </example>
        public Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true, bool asSeparateQuery = false);

        /// <summary>
        /// Persist a new entity of <see cref="IRepository{T}"></see>
        /// </summary>
        /// <param name="entity">Entity to be added</param>
        /// <returns>A tuple (bool, string). The bool represents the call success and the string any possible message.</returns>
        public Task<(bool, string?)> AddAsync(T entity);

        /// <summary>
        /// Persist a new a collection of entities of <see cref="IRepository{T}"></see>
        /// </summary>
        /// <param name="entities">List of entities to be added</param>
        /// <returns>A tuple (bool, string). The bool represents the call success and the string any possible message.</returns>
        public Task<(bool, string?)> AddRangeAsync(List<T> entities, int batchSize = 1000);

        /// <summary>
        /// Updates an existing entity of <see cref="IRepository{T}"></see>
        /// </summary>
        /// <param name="entity">Updated entity</param>
        /// <returns>A tuple (bool, string). The bool represents the call success and the string any possible message.</returns>
        public (bool, string?) Update(T entity);

        /// <summary>
        /// Removes an existing entity of <see cref="IRepository{T}"></see>
        /// </summary>
        /// <param name="entity">Entity to be removed</param>
        /// <returns>A tuple (bool, string). The bool represents the call success and the string any possible message.</returns>
        public (bool, string?) Remove(T entity);

        /// <summary>
        /// Removes a collection of entities of <see cref="IRepository{T}"></see>
        /// </summary>
        /// <param name="entities">List of entities to be removed</param>
        /// <returns>A tuple (bool, string). The bool represents the call success and the string any possible message.</returns>
        public (bool, string?) RemoveRange(List<T> entities);

        /// <summary>
        /// Deletes all entities of type T.
        /// </summary>
        /// <returns>A tuple (bool, string). The bool represents the call success, and the string contains any possible error message.</returns>
        public Task<(bool, string?)> DeleteAllAsync();

        /// <summary>
        /// Updates all entities passed as an argument.
        /// </summary>
        /// <returns>A tuple (bool, string). The bool represents the call success, and the string contains any possible error message.</returns>
        public Task<(bool, string?)> UpdateRangeAsync(IEnumerable<T> entities);

    }
}