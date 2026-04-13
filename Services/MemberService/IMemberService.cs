using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Members;
using GaguaGym.DTOs.Requests.PaginationRequest;
using GaguaGym.DTOs.Responses.Member;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.DTOs.Responses.Visit;

namespace GaguaGym.Services.MemberService
{
    public interface IMemberService
    {
        Result<PagedResult<MemberListResponse>> GetAll(PaginationRequest request);
        Result<MemberResponse> GetById(int id);
        Result<MemberResponse> Create(CreateMemberRequest request);
        Result<MemberResponse> Update(int id, UpdateMemberRequest request);
        Result<bool> Delete(int id);
        Result<List<VisitResponse>> GetVisits(int memberId);
        Result<List<MembershipResponse>> GetMemberships(int memberId);
    }
}
