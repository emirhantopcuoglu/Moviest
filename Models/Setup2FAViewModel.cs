using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class Setup2FAViewModel
    {
        public string? AuthenticatorKey { get; set; }
        public string? QrCodeDataUrl { get; set; }

        [Required(ErrorMessage = "Doğrulama kodu gereklidir.")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Geçerli bir 6 haneli kod girin.")]
        [DataType(DataType.Text)]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;
    }
}
