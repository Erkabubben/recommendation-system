namespace RecommendationSystemAPI.Models
{
    public class TopMatchingUserRequest
    {
        public string User { get; set; }
        public string Similarity { get; set; }
        public int Results { get; set; }
    }

    public class TopMatchingUserResponse
    {
        public string[] Users { get; set; }
        public double[] Similarities { get; set; }

        public TopMatchingUserResponse(string[] users, double[] similarities)
        {
            Users = users;
            Similarities = similarities;
        }
    }

    public class UserNamesListResponse
    {
        public List<string> UserNames { get; set; }
        public UserNamesListResponse(List<string> userNames) => UserNames = userNames;
    }

    public class MovieRecommendationsResponse
    {
        public List<string> Movies { get; set; }
        public List<string> Ids { get; set; }
        public List<string> Scores { get; set; }
        public MovieRecommendationsResponse(List<string> movies, List<string> ids, List<string> scores)
        {
            Movies = movies;
            Ids = ids;
            Scores = scores;
        }
    }
}
