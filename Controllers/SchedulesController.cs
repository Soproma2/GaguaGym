using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Schedules;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.Services.ScheduleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        public SchedulesController(IScheduleService scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<ScheduleResponse>>), 200)]
        public IActionResult GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = _scheduleService.GetAll(from, to);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 200)]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 404)]
        public IActionResult GetById(int id)
        {
            var result = _scheduleService.GetById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trainer")]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 201)]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 400)]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 404)]
        public IActionResult Create(CreateScheduleRequest req)
        {
            var result = _scheduleService.Create(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Trainer")]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 200)]
        [ProducesResponseType(typeof(Result<ScheduleResponse>), 404)]
        public IActionResult Update(int id, UpdateScheduleRequest req)
        {
            var result = _scheduleService.Update(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 404)]
        public IActionResult Delete(int id)
        {
            var result = _scheduleService.Delete(id);
            return StatusCode(result.StatusCode, result);
        }
    }

}
