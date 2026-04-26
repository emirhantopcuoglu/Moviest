using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanici adi gereklidir.")]
        [Display(Name = "Kullanici Adi")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Sifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatirla")]
        public bool RememberMe { get; set; }
    }
}
