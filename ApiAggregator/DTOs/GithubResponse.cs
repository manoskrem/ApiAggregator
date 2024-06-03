namespace ApiAggregator.DTOs
{
    public class GithubResponse
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Blog { get; set; }
        public int PublicRepos { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
    }
}
