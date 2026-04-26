using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class ActorDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty;
        public string Deathday { get; set; } = string.Empty;

        [JsonPropertyName("profile_path")]
        public string ProfilePath { get; set; } = string.Empty;

        public string PlaceOfBirth { get; set; } = string.Empty;
    }
}
