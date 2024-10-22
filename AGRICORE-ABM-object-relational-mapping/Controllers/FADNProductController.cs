using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing FADN product entities.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FADNProductController : ControllerBase
    {

        private readonly IRepository<FADNProduct> _repositoryFADNProduct;
        private readonly ILogger<FADNProductController> _logger;

        public FADNProductController(
            IRepository<FADNProduct> repositoryFADNProduct,
            ILogger<FADNProductController> logger
        )
        {
            _repositoryFADNProduct = repositoryFADNProduct;
            _logger = logger;
        }

        /// <summary>
        /// Adds a single FADNProduct to the database.
        /// </summary>
        /// <param name="product">The FADNProduct object to add.</param>
        /// <returns>Returns the added FADNProduct on success.</returns>
        [HttpPost("/FADNproducts/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FADNProduct>> AddFADNProduct(FADNProduct product)
        {
            var(success, message) = await _repositoryFADNProduct.AddAsync(product);
            if (success)
            {
                _logger.LogInformation($"FADNProduct {product.Id} added");
                return CreatedAtAction(nameof(AddFADNProduct), new { id = product.Id }, product);
            }
            else
            {
                _logger.LogError("Error while inserting FADNProduct: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds a range of FADNProducts to the database.
        /// </summary>
        /// <param name="products">List of FADNProduct objects to add.</param>
        /// <returns>Returns the added FADNProducts on success.</returns>
        [HttpPost("/FADNProducts/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FADNProduct>> AddFADNProductRange(List<FADNProduct> products)
        {
            var (success, message) = await _repositoryFADNProduct.AddRangeAsync(products);
            if (success)
            {
                var createdIds = products.Select(x => x.Id).ToList();
                _logger.LogInformation($"FADNProduct {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddFADNProductRange), new { }, products);
            }
            else
            {
                _logger.LogError("Error while inserting FADNProducts: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all FADNProducts from the database.
        /// </summary>
        /// <returns>Returns a list of FADNProducts on success.</returns>
        [HttpGet("/FADNProducts/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IAsyncEnumerable<FADNProduct>>> GetAllFADNProducts()
        {
            var result = await _repositoryFADNProduct.GetAllAsync();
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no FADNProducts in the database");
                return new NoContentResult();
            }
            return Ok(result);
        }
    }
}
