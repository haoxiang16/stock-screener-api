using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using StockAPI.Models;
using StockAPI.Models.DTOs;
using StockAPI.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace StockAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly StockContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(StockContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDTO
            {
                Token = token,
                Username = user.Username,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes)
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequestDTO request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            var hashedPassword = HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}