using GaguaGym.Common;

namespace GaguaGym.Services.ScheduleService
{
    public interface IScheduleService
    {
        Result<List<ScheduleResponse>> GetAll(DateTime? from, DateTime? to);
        Result<ScheduleResponse> GetById(int id);
        Result<ScheduleResponse> Create(CreateScheduleRequest request);
        Result<ScheduleResponse> Update(int id, UpdateScheduleRequest request);
        Result<bool> Delete(int id);
    }
}
