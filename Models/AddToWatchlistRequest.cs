using System.ComponentModel.DataAnnotations;

namespace Moviest.Models
{
    public class AddToWatchlistRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz film kimliği.")]
        public int MovieId { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
