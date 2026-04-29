namespace Moviest.Constants
{
    internal static class TmdbEndpoints
    {
        internal const string PopularMovies = "movie/popular";
        internal const string NowPlaying = "movie/now_playing";
        internal const string TopRated = "movie/top_rated";
        internal const string Upcoming = "movie/upcoming";
        internal const string TrendingMovies = "trending/movie/week";
        internal const string MovieDetail = "movie/{0}";
        internal const string MovieCredits = "movie/{0}/credits";
        internal const string MovieVideos = "movie/{0}/videos";
        internal const string SimilarMovies = "movie/{0}/similar";
        internal const string DiscoverByGenre = "discover/movie";
        internal const string GenreList = "genre/movie/list";
        internal const string SearchMovie = "search/movie";
        internal const string PersonDetail = "person/{0}";
        internal const string PersonMovieCredits = "person/{0}/movie_credits";
    }

    internal static class ConfigKeys
    {
        internal const string ApiBaseUrl = "ApiSettings:BaseUrl";
        internal const string ApiKey = "ApiSettings:Key";
        internal const string AdminEmail = "AdminCredentials:Email";
        internal const string AdminPassword = "AdminCredentials:Password";
        internal const string DefaultConnection = "ConnectionStrings:DefaultConnection";
    }

    internal static class TempDataKeys
    {
        internal const string Error = "Error";
        internal const string Success = "Success";
    }
}
