using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Yeni Şifre Tekrar")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
