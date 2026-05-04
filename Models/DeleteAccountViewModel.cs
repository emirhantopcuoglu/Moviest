using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class DeleteAccountViewModel
    {
        [Required(ErrorMessage = "Şifrenizi girmeniz gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
