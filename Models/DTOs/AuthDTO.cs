namespace StockAPI.Models.DTOs
{
    public class LoginRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class RegisterRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
} 