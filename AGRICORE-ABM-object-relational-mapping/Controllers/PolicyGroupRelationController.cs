using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing policy group relations within populations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PolicyGroupRelationController : ControllerBase
    {

        private readonly IRepository<Population> _repositoryPopulation;
        private readonly IRepository<PolicyGroupRelation> _repositoryPolicyGroupRelation;
        private readonly ILogger<PolicyGroupRelationController> _logger;

        public PolicyGroupRelationController(
            IRepository<Population> repositoryPopulation,
            IRepository<PolicyGroupRelation> repositoryPolicyGroupRelation,
            ILogger<PolicyGroupRelationController> logger
        )
        {
            _repositoryPopulation = repositoryPopulation;
            _repositoryPolicyGroupRelation = repositoryPolicyGroupRelation;
            _logger = logger;
        }


        /// <summary>
        /// Adds a new policy group relation for a specific population.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <param name="relation">Policy group relation to add.</param>
        /// <returns>The added policy group relation.</returns>
        [HttpPost("/population/{populationId}/policyProductGroupRelation/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PolicyGroupRelation>> AddPolicyGroupRelation(long populationId, PolicyGroupRelation relation)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId ,include: p => p
                .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.ProductGroup)
                .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.Policy));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(409, error);
            }

            relation.PopulationId = populationId;

            var relationExists = existingPopulation.PolicyGroupRelations?.Find(pgr => pgr.ProductGroup.Id == relation.ProductGroupId && pgr.PolicyId == relation.PolicyId);//await _repositoryPolicyGroupRelation.GetSingleOrDefaultAsync(r => r.ProductGroupId == relation.ProductGroupId && r.PolicyId == relation.PolicyId && relation.PopulationId == populationId) != null;

            if (relationExists != null)
            {
                error = "This relation already exists.";
                _logger.LogError(error);
                return BadRequest(error);
            }

            var(success,message) = await _repositoryPolicyGroupRelation.AddAsync(relation);
            if (success)
            {
                _logger.LogInformation($"PolicyGroupRelation {relation.Id} added");
                return CreatedAtAction(nameof(AddPolicyGroupRelation), new { id = relation.Id }, relation);
            }
                
            else
            {
                _logger.LogError("Errro while inserting PolicyGroupRelation: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds multiple policy group relations for a specific population.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <param name="relations">List of policy group relations to add.</param>
        /// <returns>The added policy group relations.</returns>
        [HttpPost("/population/{populationId}/policyProductGroupRelation/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PolicyGroupRelation>> AddPolicyGroupRelationRange(long populationId, List<PolicyGroupRelation> relations)
        {

            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId, include: p => p
                .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.ProductGroup)
                .Include(p => p.PolicyGroupRelations).ThenInclude(pgr => pgr.Policy));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogError(error);
                return StatusCode(409, error);
            }

            relations.ForEach(r => r.PopulationId = populationId);
            
            var relationExists = existingPopulation.PolicyGroupRelations?.FindAll(pgr => relations.Contains(pgr));//await _repositoryPolicyGroupRelation.GetAllAsync(r => relations.Any(r2 => r2.ProductGroupId == r.ProductGroupId && r2.PolicyId == r.PolicyId && r2.PopulationId == r.PopulationId));

            if (relationExists != null)
            {
                error = "One of the relations already exists.";
                _logger.LogError(error);
                return BadRequest();
            }

            var(success,message)  = await _repositoryPolicyGroupRelation.AddRangeAsync(relations);
            if (success)
            {
                var createdIds = relations.Select(r => r.Id).ToList();
                _logger.LogInformation($"PolicyGroupRelations {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddPolicyGroupRelationRange), new { }, relations);
            }
            else
            {
                _logger.LogError("Error while inserting PolicyGroupRelation: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all policy group relations for a specific population.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <returns>All policy group relations associated with the specified population.</returns>

        [HttpGet("/population/{populationId}/policyProductGroupRelation/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IAsyncEnumerable<PolicyGroupRelation>>> GetAllPolicyProductRelations(long populationId)
        {
            var existingPopulation = await _repositoryPopulation.GetSingleOrDefaultAsync(p => p.Id == populationId ,include: p => p.Include(p => p.PolicyGroupRelations));
            string error = string.Empty;
            if (existingPopulation == null)
            {
                error = "This population does not exist";
                _logger.LogInformation(error);
                return StatusCode(409, error);
            }

            var result = existingPopulation.PolicyGroupRelations; //await _repositoryPolicyGroupRelation.GetAllAsync(r => r.PopulationId == populationId);

            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no PolicyGroupRelation in this population");
                return new NoContentResult();
            }

            return Ok(result);
        }

    }
}
