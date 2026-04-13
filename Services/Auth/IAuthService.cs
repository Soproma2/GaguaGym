using GaguaGym.Common;
using Microsoft.AspNetCore.Identity.Data;

namespace GaguaGym.Services.Auth
{
    public interface IAuthService
    {
        Result<AuthResponse> Register(RegisterRequest request);
        Result<AuthResponse> Login(LoginRequest request);
        Result<bool> ChangePassword(int userId, ChangePasswordRequest request);
    }
}
