using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Yeni e-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "Yeni E-posta")]
        public string NewEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
