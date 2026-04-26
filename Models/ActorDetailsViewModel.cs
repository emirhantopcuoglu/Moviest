namespace Moviest.Models
{
    public class ActorDetailsViewModel
    {
        public ActorDetails Actor { get; set; } = new();
        public List<Movie> Movies { get; set; } = [];
    }
}
