using GaguaGym.Common;

namespace GaguaGym.Services.MembershipService
{
    public interface IMembershipService
    {
        Result<List<MembershipPlanResponse>> GetAllPlans();
        Result<MembershipPlanResponse> GetPlanById(int id);
        Result<MembershipPlanResponse> CreatePlan(CreateMembershipPlanRequest request);
        Result<MembershipPlanResponse> UpdatePlan(int id, UpdateMembershipPlanRequest request);
        Result<MemberMembershipResponse> AssignMembership(AssignMembershipRequest request);
        Result<bool> CancelMembership(int membershipId);
        Result<List<MemberMembershipResponse>> GetActiveMemberships();
    }
}
