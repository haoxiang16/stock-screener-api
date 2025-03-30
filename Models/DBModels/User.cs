namespace StockAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }  // 儲存雜湊後的密碼
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 