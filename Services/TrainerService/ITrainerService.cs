using GaguaGym.Common;

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
