using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanici adi bos birakilamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanici adi 3 ile 50 karakter arasinda olmalidir.")]
        [Display(Name = "Kullanici Adi")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi bos birakilamaz.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Sifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre onayi gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Sifreler eslesmiyor.")]
        [Display(Name = "Sifre Onayi")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
