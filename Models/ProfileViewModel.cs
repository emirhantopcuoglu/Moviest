namespace Moviest.Models
{
    public class ProfileViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
    }
}
