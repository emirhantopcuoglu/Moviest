using System.Text.Json.Serialization;

namespace Moviest.Models
{
    public class Cast
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Character { get; set; }
        
        [JsonPropertyName("profile_path")]
        public string ProfilePath { get; set; }
    }
}