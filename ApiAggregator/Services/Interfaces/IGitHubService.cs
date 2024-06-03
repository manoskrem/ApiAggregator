using ApiAggregator.DTOs;
using Refit;

namespace ApiAggregator.Services
{

    
        public interface IGitHubService
    {
        [Get("/users/{username}")]
        Task<GithubResponse> GetUserAsync([AliasAs("username")] string username);
    }
}

