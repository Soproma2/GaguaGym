using GaguaGym.Common;
using GaguaGym.DTOs.Requests.MembershipPlans;
using GaguaGym.DTOs.Responses.MembershipPlan;
using GaguaGym.Services.MembershipService;
using GaguaGym.DTOs.Responses.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;
        public MembershipsController(IMembershipService membershipService) => _membershipService = membershipService;

        [HttpGet("plans")]
        [ProducesResponseType(typeof(Result<List<MembershipPlanResponse>>), 200)]
        public IActionResult GetAllPlans()
        {
            var result = _membershipService.GetAllPlans();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("plans/{id:int}")]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 200)]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 404)]
        public IActionResult GetPlanById(int id)
        {
            var result = _membershipService.GetPlanById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("plans")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 201)]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 400)]
        public IActionResult CreatePlan(CreateMembershipPlanRequest req)
        {
            var result = _membershipService.CreatePlan(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("plans/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 200)]
        [ProducesResponseType(typeof(Result<MembershipPlanResponse>), 404)]
        public IActionResult UpdatePlan(int id, UpdateMembershipPlanRequest req)
        {
            var result = _membershipService.UpdatePlan(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<MembershipResponse>), 201)]
        [ProducesResponseType(typeof(Result<MembershipResponse>), 400)]
        [ProducesResponseType(typeof(Result<MembershipResponse>), 404)]
        public IActionResult AssignMembership(AssignMembershipRequest req)
        {
            var result = _membershipService.AssignMembership(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        [ProducesResponseType(typeof(Result<bool>), 404)]
        public IActionResult CancelMembership(int id)
        {
            var result = _membershipService.CancelMembership(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<List<MembershipResponse>>), 200)]
        public IActionResult GetActiveMemberships()
        {
            var result = _membershipService.GetActiveMemberships();
            return StatusCode(result.StatusCode, result);
        }
    }
}
