using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Visits;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.Services.VisitService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitService;
        public VisitsController(IVisitService visitService) => _visitService = visitService;

        [HttpPost("check-in")]
        [ProducesResponseType(typeof(Result<VisitResponse>), 201)]
        [ProducesResponseType(typeof(Result<VisitResponse>), 400)]
        [ProducesResponseType(typeof(Result<VisitResponse>), 404)]
        public IActionResult CheckIn(CheckInRequest req)
        {
            var result = _visitService.CheckIn(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("check-out/{visitId:int}")]
        [ProducesResponseType(typeof(Result<VisitResponse>), 200)]
        [ProducesResponseType(typeof(Result<VisitResponse>), 400)]
        [ProducesResponseType(typeof(Result<VisitResponse>), 404)]
        public IActionResult CheckOut(int visitId)
        {
            var result = _visitService.CheckOut(visitId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("today")]
        [ProducesResponseType(typeof(Result<List<VisitResponse>>), 200)]
        public IActionResult GetToday()
        {
            var result = _visitService.GetTodayVisits();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("member/{memberId:int}")]
        [Authorize(Roles = "Admin,Member")]
        [ProducesResponseType(typeof(Result<List<VisitResponse>>), 200)]
        [ProducesResponseType(typeof(Result<List<VisitResponse>>), 404)]
        public IActionResult GetMemberVisits(int memberId)
        {
            var result = _visitService.GetMemberVisits(memberId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
