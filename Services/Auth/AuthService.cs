using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GaguaGym.Services.Auth
{
    public class AuthService(AppDbContext db, IConfiguration configuration) : IAuthService
    {
        public Result<AuthResponse> Register(RegisterRequest request)
        {
            var exists = db.Users.Any(u => u.Email == request.Email.ToLower());
            if (exists)
                return Result<AuthResponse>.Failure("ელ.ფოსტა უკვე დარეგისტრირებულია.");

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges();

            if (request.Role == Enums.UserRole.Member)
            {
                db.Members.Add(new Member { UserId = user.Id });
                db.SaveChanges();
            }

            var token = GenerateToken(user);
            return Result<AuthResponse>.Success(MapToAuthResponse(user, token), 201);
        }

        public Result<AuthResponse> Login(LoginRequest request)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == request.Email.ToLower());

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("ელ.ფოსტა ან პაროლი არასწორია.", 401);

            if (!user.IsActive)
                return Result<AuthResponse>.Failure("ანგარიში გათიშულია.", 403);

            var token = GenerateToken(user);
            return Result<AuthResponse>.Success(MapToAuthResponse(user, token));
        }

        public Result<bool> ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = db.Users.Find(userId);
            if (user is null)
                return Result<bool>.NotFound("მომხმარებელი ვერ მოიძებნა.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("მიმდინარე პაროლი არასწორია.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Result<bool>.Success(true);
        }

        private string GenerateToken(User user)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationHours = int.Parse(jwtSettings["ExpirationInHours"] ?? "24");

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
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
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
