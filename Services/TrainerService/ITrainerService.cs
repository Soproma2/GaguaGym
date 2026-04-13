using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Trainers;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.DTOs.Responses.Trainer;

namespace GaguaGym.Services.TrainerService
{
    public interface ITrainerService
    {
        Result<List<TrainerResponse>> GetAll();
        Result<TrainerResponse> GetById(int id);
        Result<TrainerResponse> Create(CreateTrainerRequest request);
        Result<TrainerResponse> Update(int id, UpdateTrainerRequest request);
        Result<List<ScheduleResponse>> GetSchedules(int trainerId);
    }
}
