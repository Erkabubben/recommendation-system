using System;
using System.Text;
using System.Diagnostics;

class Program
{
    static string appFolderPath = 
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    (Path.GetDirectoryName(
                        AppDomain.CurrentDomain.BaseDirectory)))));

    private static List<User> _users = new List<User>();
    private static List<Movie> _movies = new List<Movie>();
    private static List<MovieRating> _movieRatings = new List<MovieRating>();

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Recommendation System!");
        ReadCSVs();

        string selectedUserName = "Toby";
        string similarity = "Euclidean";
        similarity = "Pearson";

        int results = 3;

        User selectedUser = _users.Find(user => user.Name == selectedUserName);
        Func<User, User, double> similarityFunc = (similarity == "Euclidean") ? CalculateUserSimilarityEuclidean : CalculateUserSimilarityPearson;

        var topMatchingUsers = GetTopMatchingUsers(selectedUser, similarityFunc);
        foreach (var userSimilarity in topMatchingUsers)
            Console.WriteLine($"{userSimilarity.Item1.Name} : {userSimilarity.Item2}");
        var topRecommendedMovies = GetRecommendationsForUser(selectedUser, similarityFunc);
        foreach (var movieRecommendation in topRecommendedMovies)
            Console.WriteLine($"{movieRecommendation.Item1.Title} : {movieRecommendation.Item2}");
    }

    private static List<(User, double)> GetTopMatchingUsers(User userA, Func<User, User, double> similarityFunc)
    {
        var list = new List<(User, double)>();
        foreach (var user in _users)
        {
            if (user == userA)
                continue;

            list.Add((user, similarityFunc(userA, user)));
        }
        list.Sort((a, b) => (a.Item2 < b.Item2) ? 1 : -1);
        return list;
    }

    private static double CalculateUserSimilarityEuclidean(User userA, User userB)
    {
        // Init variables.
        double similarity = 0;
        int numberOfMatchingProducts = 0;
        var ratingsByUserA = userA.GetRatingsByUser();
        var ratingsByUserB = userB.GetRatingsByUser();
        // Iterate over all rating combinations.
        foreach (var movieRatingA in ratingsByUserA)
        {
            foreach (var movieRatingB in ratingsByUserB)
            {
                if (movieRatingA.MovieId == movieRatingB.MovieId)
                {
                    similarity += Math.Pow(movieRatingA.Rating - movieRatingB.Rating, 2);
                    numberOfMatchingProducts += 1;
                }
            }
        }
        // Return 0 if no ratings in common.
        if (numberOfMatchingProducts == 0)
            return 0;
        // Calculate inverted score.
        double inverted = 1 / (1 + similarity);
        return inverted;
    }

    private static double CalculateUserSimilarityPearson(User userA, User userB)
    {
        // Init variables.
        double sum1 = 0;
        double sum2 = 0;
        double sum1sq = 0;
        double sum2sq = 0;
        double pSum = 0;
        int numberOfMatchingProducts = 0;
        var ratingsByUserA = userA.GetRatingsByUser();
        var ratingsByUserB = userB.GetRatingsByUser();
        // Iterate over all rating combinations.
        foreach (var movieRatingA in ratingsByUserA)
        {
            foreach (var movieRatingB in ratingsByUserB)
            {
                if (movieRatingA.MovieId == movieRatingB.MovieId)
                {
                    sum1 += movieRatingA.Rating;
                    sum2 += movieRatingB.Rating;
                    sum1sq += Math.Pow(movieRatingA.Rating, 2);
                    sum2sq += Math.Pow(movieRatingB.Rating, 2);
                    pSum += movieRatingA.Rating * movieRatingB.Rating;
                    numberOfMatchingProducts += 1;
                }
            }
        }
        // Return 0 if no ratings in common.
        if (numberOfMatchingProducts == 0)
            return 0;
        // Calculate Pearson.
        double num = pSum - (sum1 * sum2 / numberOfMatchingProducts);
        double den = Math.Sqrt((sum1sq - Math.Pow(sum1, 2) / numberOfMatchingProducts) * (sum2sq - Math.Pow(sum2, 2) / numberOfMatchingProducts));
        return num / den;
    }

    private static List<(Movie, double)> GetRecommendationsForUser(User userA, Func<User, User, double> similarityFunc)
    {
        var topMatchingUsers = GetTopMatchingUsers(userA, similarityFunc);
        var weightedScoresTable = new double[_movies.Count, topMatchingUsers.Count];
        var weightedScoresTotal = new double[_movies.Count];
        var sumOfSimilarities = new double[_movies.Count];
        var finalScores = new double[_movies.Count];
        // Populate weighted scores table.
        for (int y = 0; y < topMatchingUsers.Count; y++)
        {
            var matchingUser = topMatchingUsers[y];
            if (matchingUser.Item2 <= 0)
            {
                for (int x = 0; x < _movies.Count; x++)
                    weightedScoresTable[x, y] = -1;
                continue;
            }

            var ratingsByUser = matchingUser.Item1.GetRatingsByUser();
            for (int x = 0; x < _movies.Count; x++)
            {
                var movieRating = ratingsByUser.Find((movieRating) => movieRating.MovieId == _movies[x].Id);
                if (movieRating != null)
                    weightedScoresTable[x, y] = matchingUser.Item2 * movieRating.Rating;
                else
                    weightedScoresTable[x, y] = -1;
            }
        }
        // Calculate weightedScoresTotals and similarityTotals.
        for (int x = 0; x < _movies.Count; x++)
        {
            double weightedScoreTotal = 0;
            for (int y = 0; y < topMatchingUsers.Count; y++)
            {
                if (weightedScoresTable[x, y] != -1)
                    weightedScoreTotal += weightedScoresTable[x, y];
            }
            weightedScoresTotal[x] = weightedScoreTotal;

            double similarityTotal = 0;
            for (int y = 0; y < topMatchingUsers.Count; y++)
            {
                if (weightedScoresTable[x, y] != -1)
                    similarityTotal += topMatchingUsers[y].Item2;
            }
            sumOfSimilarities[x] = similarityTotal;
            finalScores[x] = weightedScoresTotal[x] / sumOfSimilarities[x];
        }
        // Prepare list to be returned.
        var moviesWithScoresList = new List<(Movie, double)>();
        var ratingsByUserA = userA.GetRatingsByUser();
        for (int x = 0; x < _movies.Count; x++)
        {
            if (ratingsByUserA.FindIndex((movieRating) => movieRating.MovieId == _movies[x].Id) == -1)
                moviesWithScoresList.Add((_movies[x], finalScores[x]));
        }
        moviesWithScoresList.Sort((a, b) => (a.Item2 < b.Item2) ? 1 : -1);
        return moviesWithScoresList;
    }

    private class User
    {
        private int _id;
        private string _name;

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public User(int id, string name) { _id = id; _name = name;}
        public List<MovieRating> GetRatingsByUser()
        {
            var list = new List<MovieRating>();
            foreach (var movieRating in _movieRatings)
            {
                if (movieRating.UserId == Id)
                    list.Add(movieRating);
            }
            return list;
        }
    }

    private class Movie
    {
        private int _id;
        private string _title;
        private int _year;

        public int Id { get => _id; set => _id = value; }
        public string Title { get => _title; set => _title = value; }
        public int Year { get => _year; set => _year = value; }

        public Movie(int id, string title, int year) { _id = id; _title = title; _year = year;}
    }

    private class MovieRating
    {
        private int _userId;
        private int _movieId;
        private double _rating;

        public MovieRating(int userId, int movieId, double rating) { _userId = userId; _movieId = movieId; _rating = rating; }

        public int UserId { get => _userId; set => _userId = value; }
        public int MovieId { get => _movieId; set => _movieId = value; }
        public double Rating { get => _rating; set => _rating = value; }
    }

    static void ReadCSVs()
    {
        void ReadCSV(string path, Action<string[]> onReadLineAction)
        {
            using (var reader = new StreamReader(appFolderPath + path))
            {
                bool isFirstLine = true;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (!isFirstLine)
                        onReadLineAction(values);
                    else
                        isFirstLine = false;
                }
            }
        }

        ReadCSV(@"\datasets\example\users.csv", (values)
            => _users.Add(new User(Int32.Parse(values[0]), values[1])));
        ReadCSV(@"\datasets\example\movies.csv", (values)
            => _movies.Add(new Movie(Int32.Parse(values[0]), values[1], Int32.Parse(values[2]))));
        ReadCSV(@"\datasets\example\ratings.csv", (values)
            => _movieRatings.Add(
                new MovieRating(Int32.Parse(values[0]), Int32.Parse(values[1]), double.Parse(values[2].Replace('.', ',')))));
    }
}
