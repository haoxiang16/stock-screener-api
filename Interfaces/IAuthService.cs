using StockAPI.Models;
using StockAPI.Models.DTOs;

namespace StockAPI.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<bool> RegisterAsync(RegisterRequestDTO request);
        string GenerateJwtToken(User user);
    }
} 