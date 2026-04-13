using GaguaGym.Common;

namespace GaguaGym.Services.MemberService
{
    public class IMemberService
    {
        Result<PagedResult<MemberListResponse>> GetAll(PaginationRequest request);
        Result<MemberResponse> GetById(int id);
        Result<MemberResponse> Create(CreateMemberRequest request);
        Result<MemberResponse> Update(int id, UpdateMemberRequest request);
        Result<bool> Delete(int id);
        Result<List<VisitResponse>> GetVisits(int memberId);
        Result<List<MemberMembershipResponse>> GetMemberships(int memberId);
    }
}
