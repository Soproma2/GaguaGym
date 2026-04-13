using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Trainers;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.DTOs.Responses.Trainer;
using GaguaGym.Services.TrainerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TrainersController : ControllerBase
    {
        private readonly ITrainerService _trainerService;
        public TrainersController(ITrainerService trainerService) => _trainerService = trainerService;

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<TrainerResponse>>), 200)]
        public IActionResult GetAll()
        {
            var result = _trainerService.GetAll();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 200)]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 404)]
        public IActionResult GetById(int id)
        {
            var result = _trainerService.GetById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 201)]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 400)]
        public IActionResult Create(CreateTrainerRequest req)
        {
            var result = _trainerService.Create(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 200)]
        [ProducesResponseType(typeof(Result<TrainerResponse>), 404)]
        public IActionResult Update(int id, UpdateTrainerRequest req)
        {
            var result = _trainerService.Update(id, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/schedules")]
        [ProducesResponseType(typeof(Result<List<ScheduleResponse>>), 200)]
        [ProducesResponseType(typeof(Result<List<ScheduleResponse>>), 404)]
        public IActionResult GetSchedules(int id)
        {
            var result = _trainerService.GetSchedules(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
