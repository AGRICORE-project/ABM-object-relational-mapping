using AGRICORE_ABM_object_relational_mapping.Services;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing FADN product relations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FADNProductRelationController : ControllerBase
    {

        private readonly IRepository<FADNProductRelation> _repositoryFADNProductRelation;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly IArableService _arableService;
        private readonly ILogger<FADNProductRelationController> _logger;

        public FADNProductRelationController(
            IRepository<FADNProductRelation> repositoryFADNProductRelation,
            IRepository<ProductGroup> repositoryProductGroup,
            IRepository<FADNProduct> repositoryFADNProduct,
            IArableService arableService,
            ILogger<FADNProductRelationController> logger
        )
        {
            _repositoryFADNProductRelation = repositoryFADNProductRelation;
            _repositoryProductGroup = repositoryProductGroup;
            _repositoryFADNProduct = repositoryFADNProduct;
            _arableService = arableService;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new FADN product relation.
        /// </summary>
        /// <param name="relation">The FADN product relation object to add.</param>
        /// <returns>Returns the added FADN product relation on success.</returns>

        [HttpPost("/FADNProductRelation/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FADNProductRelation>> AddFADNProductRelations(FADNProductRelation relation)
        {
            var existingGroup = await _repositoryProductGroup.GetSingleOrDefaultAsync(pg => pg.Id == relation.ProductGroupId && pg.PopulationId == relation.PopulationId, include: pg => pg
                .Include(pg => pg.FADNProductRelations), asNoTracking: true, asSeparateQuery: true);

            string error = string.Empty;
            if (existingGroup == null)
            {
                error = $@"This product group {relation.ProductGroupId} does not exist for this population {relation.PopulationId}";
                _logger.LogError(error);
                return StatusCode(409, error);
            }

            var existingFADNProduct = await _repositoryFADNProduct.GetSingleOrDefaultAsync(p => p.Id == relation.FADNProductId);

            if (existingFADNProduct == null)
            {
                error = $@"This product  {relation.FADNProductId} does not exist";
                _logger.LogError(error);
                return StatusCode(409, error);
            }

            if (existingGroup.FADNProductRelations != null && existingGroup.FADNProductRelations.Any(r => r.FADNProductId == relation.FADNProductId))
            {
                error = "This relation already exists.";
                _logger.LogError(error);
                return BadRequest(error);
            }

            var(success, message) = await _repositoryFADNProductRelation.AddAsync(relation);
            if(success)
            {
                _logger.LogInformation($"FADNProductRelation {relation.Id} added");
                // Disabled to leave arable condition to be manually updated in the import process
                //await _arableService.UpdateProductGroupArableCondition(relation.ProductGroupId);
                return CreatedAtAction(nameof(AddFADNProductRelations), new { id = relation.Id }, relation);
            }
            else
            {
                _logger.LogError("Error while inserting FADNProductRelation: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves a specific FADN product relation by its ID.
        /// </summary>
        /// <param name="id">The ID of the FADN product relation to retrieve.</param>
        /// <returns>Returns the FADN product relation with the specified ID on success.</returns>

        [HttpGet("/FADNProductRelation/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<FADNProductRelation>>> GetFADNProductRelations(long id)
        {
            var result = await _repositoryFADNProductRelation.GetAllAsync(fpr => fpr.Id == id);
            string error = string.Empty;
            if (result == null)
            {
                error = $@"This relation {id} does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all FADN product relations for a specific product group.
        /// </summary>
        /// <param name="id">The ID of the product group to retrieve FADN product relations for.</param>
        /// <returns>Returns a list of FADN product relations for the specified product group on success.</returns>

        [HttpGet("/productGroup/{id}/FADNProductRelation/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<FADNProductRelation>>> GetAllFADNProductRelations(long id)
        {
            var result = await _repositoryProductGroup.GetSingleOrDefaultAsync(pg => pg.Id == id, include: pg => pg.Include(pg => pg.FADNProductRelations), asNoTracking: true, asSeparateQuery: true);
            string error = string.Empty;
            if (result == null)
            {
                error = $@"This group {id} does not exist";
                _logger.LogInformation(error);
                return StatusCode(404, error);
            }

            if (result.FADNProductRelations == null || result.FADNProductRelations.Count == 0)
            {
                _logger.LogInformation("There are no FADNProductRelations in the database");
                return new NoContentResult();
            }
            return Ok(result.FADNProductRelations);
        }

    }
}
