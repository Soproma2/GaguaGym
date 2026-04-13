using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Visits;
using GaguaGym.DTOs.Responses.Visit;

namespace GaguaGym.Services.VisitService
{
    public interface IVisitService
    {
        Result<VisitResponse> CheckIn(CheckInRequest request);
        Result<VisitResponse> CheckOut(int visitId);
        Result<List<VisitResponse>> GetTodayVisits();
        Result<List<VisitResponse>> GetMemberVisits(int memberId);
    }
}
