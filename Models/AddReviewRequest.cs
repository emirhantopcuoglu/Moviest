using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class AddReviewRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz film kimliği.")]
        public int MovieId { get; set; }

        [Required]
        [StringLength(400)]
        public string MovieTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yorum boş bırakılamaz.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10 ile 1000 karakter arasında olmalıdır.")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Puan 1 ile 10 arasında olmalıdır.")]
        public int Rating { get; set; }
    }
}
