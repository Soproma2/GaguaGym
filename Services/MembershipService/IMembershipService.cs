using GaguaGym.Common;
using GaguaGym.DTOs.Requests.MembershipPlans;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.DTOs.Responses.MembershipPlan;

namespace GaguaGym.Services.MembershipService
{
    public interface IMembershipService
    {
        Result<List<MembershipPlanResponse>> GetAllPlans();
        Result<MembershipPlanResponse> GetPlanById(int id);
        Result<MembershipPlanResponse> CreatePlan(CreateMembershipPlanRequest request);
        Result<MembershipPlanResponse> UpdatePlan(int id, UpdateMembershipPlanRequest request);
        Result<MembershipResponse> AssignMembership(AssignMembershipRequest request);
        Result<bool> CancelMembership(int membershipId);
        Result<List<MembershipResponse>> GetActiveMemberships();
    }
}
