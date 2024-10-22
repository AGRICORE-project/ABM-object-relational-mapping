using AGRICORE_ABM_object_relational_mapping.Services;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing product groups within populations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductGroupController : ControllerBase
    {
        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<ProductGroup> _repositoryProductGroup;
        private readonly IArableService _arableService;
        private readonly ILogger<ProductGroupController> _logger;

        public ProductGroupController(
            IRepository<Population> repositoryPopulation,
            IRepository<ProductGroup> repositoryProductGroup,
            IArableService arableService,
            ILogger<ProductGroupController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryProductGroup = repositoryProductGroup;
            _arableService = arableService;
            _logger = logger;
        }

        /// <summary>
        /// Updates the model-specific categories of a product group.
        /// </summary>
        /// <param name="id">ID of the product group to update.</param>
        /// <param name="categories">Array of updated categories.</param>
        /// <returns>The updated product group.</returns>

        [HttpPut("/Utils/UpdateProductGroupCategories/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductGroup>> UpdateProductGroupCategories(long id, [FromQuery]String[] categories)
        {
            var productGroup = await _repositoryProductGroup.GetSingleOrDefaultAsync(x => x.Id == id);
            string error = string.Empty;
            if (productGroup == null)
            {
                error = "This product group doesn't exist";
                _logger.LogError(error);
                return BadRequest(error);
            }
            productGroup.ModelSpecificCategories = categories;
            var (success, message) = _repositoryProductGroup.Update(productGroup);
            if (!success)
            {
                _logger.LogError(message);
                return BadRequest(message);
            }
            return Ok(productGroup);

        }

        /// <summary>
        /// Adds a product group to a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to add the product group to.</param>
        /// <param name="data">Product group data to add.</param>
        /// <returns>The added product group.</returns>

        [HttpPost("/population/{populationId}/productGroup/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductGroup>> AddProductGroup(long populationId, ProductGroup data)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population doesn't exist";
                _logger.LogError(error);
                return BadRequest(error);
            }

            data.PopulationId = populationId;

            var (success,message) = await _repositoryProductGroup.AddAsync(data);

            if(success)
                _logger.LogInformation($"ProductGroup {data.Id} added");
            else
            {
                _logger.LogError("Error while inserting ProductGroup: " + message);
                return BadRequest(message);
            }
            // Disabled to leave arable condition to be manually updated in the import process
            //await _arableService.UpdateProductGroupArableCondition(data.Id);

            return CreatedAtAction(nameof(AddProductGroup), new { id = data.Id }, data);
        }

        /// <summary>
        /// Adds a range of product groups to a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to add the product groups to.</param>
        /// <param name="data">List of product groups to add.</param>
        /// <returns>The added product groups.</returns>

        [HttpPost("/population/{populationId}/productGroup/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProductGroup>>> AddRangeProductGroup(long populationId, List<ProductGroup> data)
        {

            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population doesn't exist";
                _logger.LogError(error);
                return BadRequest(error);
            }

            data.ForEach(ob => ob.PopulationId = populationId);

            var(succes,message) = await _repositoryProductGroup.AddRangeAsync(data);
            if (succes)
            {
                var createdIds = data.Select(x => x.Id).ToList();
                _logger.LogInformation($"ProductGroup {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddRangeProductGroup), new { }, data);
            }
            else
            {
                _logger.LogError("Error while inserting ProductGroup: " + message);
                return BadRequest(message);
            }
            
        }

        /// <summary>
        /// Retrieves all product groups associated with a specific population.
        /// </summary>
        /// <param name="populationId">ID of the population to retrieve product groups from.</param>
        /// <returns>List of product groups.</returns>

        [HttpGet("/population/{populationId}/productGroup/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<ProductGroup>>> GetAllProductGroups(long populationId)
        {

            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId);

            if (existingPopulation == null)
            {
                return BadRequest("This population doesn't exist");
            }

            var result = await _repositoryProductGroup.GetAllAsync(ob => ob.PopulationId == populationId);
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no ProductGroups in this population");
                return new NoContentResult();
            }
            return Ok(result);
        }

    }
}
