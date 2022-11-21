using System;
using System.Text;
using System.Diagnostics;
using RecommendationSystemAPI.Models;

namespace RecommendationSystemAPI.Services
{
    class RecommendationsSystemService
    {
        static string appFolderPath =
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            AppDomain.CurrentDomain.BaseDirectory))));

        private List<User> _users = new List<User>();
        private List<Movie> _movies = new List<Movie>();
        private List<MovieRating> _movieRatings = new List<MovieRating>();

        public RecommendationsSystemService()
        {
            ReadCSVs();
        }

        public UserNamesListResponse GetUsersList()
        {
            var userNames = new List<string>();
            foreach (var user in _users)
                userNames.Add(user.Name);
            return new UserNamesListResponse(userNames);
        }

        private Func<User, User, double> GetUserSimilarityFunc(TopMatchingUserRequest req) =>
            (req.Similarity == "Euclidean") ? CalculateUserSimilarityEuclidean : CalculateUserSimilarityPearson;


        public TopMatchingUserResponse FindTopMatchingUsers(TopMatchingUserRequest topMatchingUserRequest)
        {
            var selectedUser = _users.Find(user => user.Name == topMatchingUserRequest.User);
            var userSimilarityFunc = GetUserSimilarityFunc(topMatchingUserRequest);
            var topMatchingUsers = GetTopMatchingUsers(selectedUser, userSimilarityFunc);
            var users = new string[topMatchingUsers.Count];
            var similarities = new double[topMatchingUsers.Count];
            for (int i = 0; i < topMatchingUsers.Count; i++)
            {
                users[i] = topMatchingUsers[i].Item1.Name;
                similarities[i] = topMatchingUsers[i].Item2;
            }
            return new TopMatchingUserResponse(users, similarities);
        }

        public MovieRecommendationsResponse FindMovieRecommendationsForUser(TopMatchingUserRequest topMatchingUserRequest)
        {
            var selectedUser = _users.Find(user => user.Name == topMatchingUserRequest.User);
            var userSimilarityFunc = GetUserSimilarityFunc(topMatchingUserRequest);
            var topRecommendedMovies = GetRecommendationsForUser(selectedUser, userSimilarityFunc);
            var movieNames = new List<string>();
            var movieIDs = new List<string>();
            var movieScores = new List<string>();
            for (int i = 0; i < topRecommendedMovies.Count; i++)
            {
                movieNames.Add(topRecommendedMovies[i].Item1.Title);
                movieIDs.Add(topRecommendedMovies[i].Item1.Id.ToString());
                movieScores.Add(topRecommendedMovies[i].Item2.ToString());
            }
            return new MovieRecommendationsResponse(movieNames, movieIDs, movieScores);
        }

        void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Recommendation System!");

            string selectedUserName = "Toby";
            string similarity = "Euclidean";
            similarity = "Pearson";

            int results = 3;

            User selectedUser = _users.Find(user => user.Name == selectedUserName);
            Func<User, User, double> userSimilarityFunc = (similarity == "Euclidean") ? CalculateUserSimilarityEuclidean : CalculateUserSimilarityPearson;

            var topMatchingUsers = GetTopMatchingUsers(selectedUser, userSimilarityFunc);
            foreach (var userSimilarity in topMatchingUsers)
                Console.WriteLine($"{userSimilarity.Item1.Name} : {userSimilarity.Item2}");
            var topRecommendedMovies = GetRecommendationsForUser(selectedUser, userSimilarityFunc);
            foreach (var movieRecommendation in topRecommendedMovies)
                Console.WriteLine($"{movieRecommendation.Item1.Title} : {movieRecommendation.Item2}");

            string selectedMovieName = "Superman Returns";
            Movie selectedMovie = _movies.Find(movie => movie.Title == selectedMovieName);
            Console.WriteLine(selectedMovie.Title);
            /*Func<Movie, Movie, double> movieSimilarityFunc = (similarity == "Euclidean") ? CalculateMovieSimilarityEuclidean : CalculateMovieSimilarityEuclidean;
            var topMatchingMovies = GetTopMatchingMovies(movie, movieSimilarityFunc);
            foreach (var movieSimilarity in topMatchingMovies)
                Console.WriteLine($"{movieSimilarity.Item1.Title} : {movieSimilarity.Item2}");*/
        }

        private List<(User, double)> GetTopMatchingUsers(User userA, Func<User, User, double> similarityFunc)
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

        private List<(Movie, double)> GetTopMatchingMovies(Movie movieA, Func<Movie, Movie, double> similarityFunc)
        {
            var list = new List<(Movie, double)>();
            foreach (var movie in _movies)
            {
                if (movie == movieA)
                    continue;

                list.Add((movie, similarityFunc(movieA, movie)));
            }
            list.Sort((a, b) => (a.Item2 < b.Item2) ? 1 : -1);
            return list;
        }

        private double CalculateUserSimilarityEuclidean(User userA, User userB)
            => CalculateSimilarityEuclidean(userA.GetRatingsByUser(_movieRatings), userB.GetRatingsByUser(_movieRatings));
        private double CalculateUserSimilarityPearson(User userA, User userB)
            => CalculateSimilarityPearson(userA.GetRatingsByUser(_movieRatings), userB.GetRatingsByUser(_movieRatings));
        private double CalculateMovieSimilarityEuclidean(Movie movieA, Movie movieB)
            => CalculateSimilarityEuclidean(movieA.GetRatingsOfMovie(_movieRatings), movieB.GetRatingsOfMovie(_movieRatings));

        private static double CalculateSimilarityEuclidean(List<MovieRating> ratingsA, List<MovieRating> ratingsB)
        {
            // Init variables.
            double similarity = 0;
            int numberOfMatchingProducts = 0;
            // Iterate over all rating combinations.
            foreach (var movieRatingA in ratingsA)
            {
                foreach (var movieRatingB in ratingsB)
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

        private static double CalculateSimilarityPearson(List<MovieRating> ratingsA, List<MovieRating> ratingsB)
        {
            // Init variables.
            double sum1 = 0;
            double sum2 = 0;
            double sum1sq = 0;
            double sum2sq = 0;
            double pSum = 0;
            int numberOfMatchingProducts = 0;
            // Iterate over all rating combinations.
            foreach (var movieRatingA in ratingsA)
            {
                foreach (var movieRatingB in ratingsB)
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

        private List<(Movie, double)> GetRecommendationsForMovie(Movie movieA, Func<User, User, double> similarityFunc)
        {
            var scoresTable = new double[_movies.Count, _users.Count];
            for (int y = 0; y < _users.Count; y++)
            {
                for (int x = 0; x < _movies.Count; x++)
                {

                }
            }

            return null;
        }

        private List<(Movie, double)> GetRecommendationsForUser(User userA, Func<User, User, double> similarityFunc)
        {
            var topMatchingUsers = GetTopMatchingUsers(userA, similarityFunc);
            var weightedScoresTable = new double[_movies.Count, topMatchingUsers.Count];

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
                var ratingsByUser = matchingUser.Item1.GetRatingsByUser(_movieRatings);
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
            var weightedScoresTotal = new double[_movies.Count];
            var sumOfSimilarities = new double[_movies.Count];
            var finalScores = new double[_movies.Count];
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
            var ratingsByUserA = userA.GetRatingsByUser(_movieRatings);
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
            public User(int id, string name) { _id = id; _name = name; }
            public List<MovieRating> GetRatingsByUser(List<MovieRating> movieRatings)
            {
                var list = new List<MovieRating>();
                foreach (var movieRating in movieRatings)
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

            public Movie(int id, string title, int year) { _id = id; _title = title; _year = year; }

            public List<MovieRating> GetRatingsOfMovie(List<MovieRating> movieRatings)
            {
                var list = new List<MovieRating>();
                foreach (var movieRating in movieRatings)
                {
                    if (movieRating.MovieId == Id)
                        list.Add(movieRating);
                }
                return list;
            }
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

        void ReadCSVs()
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
}
