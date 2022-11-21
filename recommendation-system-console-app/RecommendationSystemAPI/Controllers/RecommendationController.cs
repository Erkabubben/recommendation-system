using Microsoft.AspNetCore.Mvc;
using RecommendationSystemAPI.Models;
using RecommendationSystemAPI.Services;

namespace RecommendationSystemAPI.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [Route("Recommendations")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationsSystemService _recommendationSystemService;

        public RecommendationController()
        {
            _recommendationSystemService = new RecommendationsSystemService();
        }

        [HttpGet(Name = "Index")]
        public ActionResult<bool> Index()
        {
            return true;
        }

        // {baseURL}/api/recommendation/{methodName}
        [HttpPost][Route("FindTopMatchingUsers")]
        public ActionResult<TopMatchingUserResponse> FindTopMatchingUsers(TopMatchingUserRequest request)
        {
            return _recommendationSystemService.FindTopMatchingUsers(request); ;
        }

        [HttpPost]
        [Route("FindMovieRecommendationsForUser")]
        public ActionResult<MovieRecommendationsResponse> FindMovieRecommendationsForUser(TopMatchingUserRequest request)
        {
            return _recommendationSystemService.FindMovieRecommendationsForUser(request); ;
        }

        [HttpPost]
        [Route("FindMovieRecommendationsForUserItemBased")]
        public ActionResult<MovieRecommendationsResponse> FindMovieRecommendationsForUserItemBased(TopMatchingUserRequest request)
        {
            return _recommendationSystemService.FindMovieRecommendationsForUserItemBased(request); ;
        }

        [HttpGet]
        [Route("GetUsersList")]
        public ActionResult<UserNamesListResponse> GetUsersList()
        {
            return _recommendationSystemService.GetUsersList();
        }
    }
}
