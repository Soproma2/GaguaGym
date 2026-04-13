using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Auth;
using GaguaGym.DTOs.Responses.Auth;
using GaguaGym.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GaguaGym.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        public AuthService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Result<AuthResponse> Register(DTOs.Requests.Auth.RegisterRequest req)
        {
            if (_db.Users.Any(u => u.Email == req.Email.ToLower()))
                return Result<AuthResponse>.Failure("ელ.ფოსტა უკვე დარეგისტრირებულია.");

            var user = new User
            {
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                Email = req.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
                Role = req.Role,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            if (req.Role == Enums.UserRole.Member)
            {
                _db.Members.Add(new Member { UserId = user.Id });
                _db.SaveChanges();
            }

            var token = GenerateToken(user);
            return Result<AuthResponse>.Success(MapToAuthResponse(user, token), 201);
        }

        public Result<AuthResponse> Login(DTOs.Requests.Auth.LoginRequest req)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == req.Email.ToLower());

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("ელ.ფოსტა ან პაროლი არასწორია.", 401);

            if (!user.IsActive)
                return Result<AuthResponse>.Failure("ანგარიში გათიშულია.", 403);

            var token = GenerateToken(user);
            return Result<AuthResponse>.Success(MapToAuthResponse(user, token));
        }

        public Result<bool> ChangePassword(int userId, ChangePasswordRequest req)
        {
            var user = _db.Users.Find(userId);
            if (user is null)
                return Result<bool>.NotFound("მომხმარებელი ვერ მოიძებნა.");

            if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("მიმდინარე პაროლი არასწორია.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword, workFactor: 12);
            user.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Result<bool>.Success(true);
        }

        private string GenerateToken(User user)
        {
            var jwt = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationHours = int.Parse(jwt["ExpirationInHours"] ?? "24");

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim("firstName",                   user.FirstName),
            new Claim("lastName",                    user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static AuthResponse MapToAuthResponse(User user, string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt = jwtToken.ValidTo
            };
        }
    }
}
