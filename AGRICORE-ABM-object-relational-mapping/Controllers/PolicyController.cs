using AutoMapper;
using DB.Data.DTOs;
using DB.Data.Models;
using DB.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRICORE_ABM_object_relational_mapping.Controllers
{
    /// <summary>
    /// Controller for managing policies related to populations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PolicyController : ControllerBase
    {

        private readonly IRepository<Policy> _repositoryPolicy;
        private readonly IMapper _mapper;
        ILogger<PolicyController> _logger;

        public PolicyController(
            IRepository<Policy> repositoryPolicy,
            IMapper mapper,
            ILogger<PolicyController> logger
        )
        {
            _repositoryPolicy = repositoryPolicy;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="policyDTO">DTO representing the policy to add.</param>
        /// <returns>The added policy.</returns>
        [HttpPost("/policies/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PolicyJsonDTO>> AddPolicy(PolicyJsonDTO policyDTO )
        {
            var existingPolicy = await _repositoryPolicy.GetSingleOrDefaultAsync(p => p.PolicyIdentifier == policyDTO.PolicyIdentifier && p.PopulationId == policyDTO.PopulationId) != null;
            string error = string.Empty;
            if (existingPolicy)
            {
                error = "This policy already exists for this population";
                _logger.LogError(error);
                return BadRequest(error);
            }

            Policy policy = _mapper.Map<Policy>(policyDTO);

            var(success,message) = await _repositoryPolicy.AddAsync(policy);
            if (success)
            {
                _logger.LogInformation($"Policy {policy.Id} added");
                return CreatedAtAction(nameof(AddPolicy), new { id = policy.Id }, policy);
            }
            else
            {
                _logger.LogError("Error while inserting Policy: "+message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Adds multiple policies.
        /// </summary>
        /// <param name="policiesDTOS">List of DTOs representing the policies to add.</param>
        /// <returns>The added policies.</returns>
        [HttpPost("/policies/addRange")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PolicyJsonDTO>> AddPolicyRange(List<PolicyJsonDTO> policiesDTOS)
        {
            var includedPopulationIdentifiersPairs = policiesDTOS.Select(q => (q.PopulationId, q.PolicyIdentifier)).ToList();
            var existingPolicies = await _repositoryPolicy.GetAllAsync();
            existingPolicies = existingPolicies.Where(p => includedPopulationIdentifiersPairs.Contains((p.PopulationId, p.PolicyIdentifier))).ToList();
            string error = string.Empty;
            if (existingPolicies.Any())
            {
                error = "One or more policies already exist.";
                _logger.LogError(error);
                return BadRequest(error);
            }
            List<Policy> policies = _mapper.Map<List<Policy>>(policiesDTOS);

            var(success,message) = await _repositoryPolicy.AddRangeAsync(policies);
            if (success)
            {
                var createdIds = policies.Select(x => x.Id).ToList();
                _logger.LogInformation($"Policies {String.Join(",",createdIds)} added");
                return CreatedAtAction(nameof(AddPolicyRange), new { }, _mapper.Map<List<PolicyJsonDTO>>(policies));
            }
            else
            {
                _logger.LogError("Error while inserting Policy: " + message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Retrieves all policies for a specific population.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <returns>All policies associated with the specified population.</returns>

        [HttpGet("/population/{populationId}/policies/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IAsyncEnumerable<PolicyJsonDTO>>> GetPopulationPolicies(long populationId)
        {
            var result = await _repositoryPolicy.GetAllAsync(predicate: p => p.PopulationId == populationId);
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no Policies in the database for that population");
                return new NoContentResult();
            }

            return Ok(_mapper.Map<List<PolicyJsonDTO>>(result));
        }

        /// <summary>
        /// Retrieves policies for a specific population formatted for UI display.
        /// </summary>
        /// <param name="populationId">Population ID.</param>
        /// <returns>Policies formatted for UI display associated with the specified population.</returns>

        [HttpGet("/population/{populationId}/policies/getForUI")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IAsyncEnumerable<PolicyForUIDTO>>> GetPopulationPoliciesForUI(long populationId)
        {
            var result = await _repositoryPolicy.GetAllAsync(predicate: p => p.PopulationId == populationId, include: q => q.Include(q => q.PolicyGroupRelations).ThenInclude(q => q.ProductGroup));
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no Policies in the database for that population");
                return new NoContentResult();
            }

            return Ok(_mapper.Map<List<PolicyForUIDTO>>(result));
        }

        /// <summary>
        /// Retrieves all policies.
        /// </summary>
        /// <returns>All policies stored in the database.</returns>

        [HttpGet("/policies/get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IAsyncEnumerable<PolicyJsonDTO>>> GetAllPolicies()
        {
            var result = await _repositoryPolicy.GetAllAsync();
            if (result == null || result.Count == 0)
            {
                _logger.LogInformation("There are no POlicies in the database");
                return new NoContentResult();
            }

            return Ok(_mapper.Map<List<PolicyJsonDTO>>(result));
        }

        /// <summary>
        /// Retrieves a policy by its identifier.
        /// </summary>
        /// <param name="policyIdentifier">Policy identifier.</param>
        /// <returns>The policy matching the specified identifier.</returns>

        [HttpGet("/policies/get/{policyIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Policy>> GetPolicyByIdentifier(string policyIdentifier)
        {
            var result = await _repositoryPolicy.GetSingleOrDefaultAsync(p => p.PolicyIdentifier == policyIdentifier);
            if (result == null)
            {
                _logger.LogInformation("This policy des not exist");
                return new NoContentResult();
            }

            return Ok(_mapper.Map<List<PolicyJsonDTO>>(result));
        }

    }
}
