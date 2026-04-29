using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class TwoFactorLoginViewModel
    {
        [Required(ErrorMessage = "Doğrulama kodu gereklidir.")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
        [DataType(DataType.Text)]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public bool RememberMachine { get; set; }
    }
}
