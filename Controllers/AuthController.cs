using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Auth;
using GaguaGym.DTOs.Responses.Auth;
using GaguaGym.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        [ProducesResponseType(typeof(Result<AuthResponse>), 201)]
        [ProducesResponseType(typeof(Result<AuthResponse>), 400)]
        public IActionResult Register(RegisterRequest req)
        {
            var result = _authService.Register(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(Result<AuthResponse>), 200)]
        [ProducesResponseType(typeof(Result<AuthResponse>), 400)]
        [ProducesResponseType(typeof(Result<AuthResponse>), 401)]
        public IActionResult Login(LoginRequest req)
        {
            var result = _authService.Login(req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        [ProducesResponseType(typeof(Result<bool>), 404)]
        public IActionResult ChangePassword(ChangePasswordRequest req)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = _authService.ChangePassword(userId, req);
            return StatusCode(result.StatusCode, result);
        }
    }
}
