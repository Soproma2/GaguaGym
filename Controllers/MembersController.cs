using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Members;
using GaguaGym.DTOs.Requests.PaginationRequest;
using GaguaGym.DTOs.Responses.Member;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.Services.MemberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MembersController(IMemberService memberService) => _memberService = memberService;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<PagedResult<MemberListResponse>>), 200)]
        public IActionResult GetAll([FromQuery] PaginationRequest req)
        {
            var result = _memberService.GetAll(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Member")]
        [ProducesResponseType(typeof(Result<MemberResponse>), 200)]
        [ProducesResponseType(typeof(Result<MemberResponse>), 404)]
        public IActionResult GetById(int id)
        {
            var result = _memberService.GetById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<MemberResponse>), 201)]
        [ProducesResponseType(typeof(Result<MemberResponse>), 400)]
        public IActionResult Create(CreateMemberRequest req)
        {
            var result = _memberService.Create(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<MemberResponse>), 200)]
        [ProducesResponseType(typeof(Result<MemberResponse>), 400)]
        [ProducesResponseType(typeof(Result<MemberResponse>), 404)]
        public IActionResult Update(int id, UpdateMemberRequest req)
        {
            var result = _memberService.Update(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 404)]
        public IActionResult Delete(int id)
        {
            var result = _memberService.Delete(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/visits")]
        [Authorize(Roles = "Admin,Member")]
        [ProducesResponseType(typeof(Result<List<VisitResponse>>), 200)]
        [ProducesResponseType(typeof(Result<List<VisitResponse>>), 404)]
        public IActionResult GetVisits(int id)
        {
            var result = _memberService.GetVisits(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/memberships")]
        [Authorize(Roles = "Admin,Member")]
        [ProducesResponseType(typeof(Result<List<MembershipResponse>>), 200)]
        [ProducesResponseType(typeof(Result<List<MembershipResponse>>), 404)]
        public IActionResult GetMemberships(int id)
        {
            var result = _memberService.GetMemberships(id);
            return StatusCode(result.StatusCode, result);
        }
    }

}
