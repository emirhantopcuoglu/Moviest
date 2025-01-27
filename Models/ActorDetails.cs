using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class ActorDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Biography { get; set; }
        public string Birthday { get; set; }
        public string Deathday { get; set; }
        
        [JsonPropertyName("profile_path")]
        public string ProfilePath { get; set; }
        public string PlaceOfBirth { get; set; }

    }
}