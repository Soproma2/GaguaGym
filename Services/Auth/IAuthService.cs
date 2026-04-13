using GaguaGym.Common;
using GaguaGym.DTOs.Responses.Auth;
using Microsoft.AspNetCore.Identity.Data;

namespace GaguaGym.Services.Auth
{
    public interface IAuthService
    {
        Result<AuthResponse> Register(DTOs.Requests.Auth.RegisterRequest request);
        Result<AuthResponse> Login(DTOs.Requests.Auth.LoginRequest request);
        Result<bool> ChangePassword(int userId, DTOs.Requests.Auth.ChangePasswordRequest request);
    }
}
