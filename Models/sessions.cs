namespace MauiApp1.Models
{
    public class Session
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public int UserId { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime LastActivity { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
