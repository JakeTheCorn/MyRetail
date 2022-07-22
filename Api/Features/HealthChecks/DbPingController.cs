using System.Linq;
using Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.HealthChecks
{
    [ApiController]
    [Route("/db-ping")]
    public class DbPingController : Controller
    {
        private readonly IDbClient _dbClient;

        public DbPingController(IDbClient dbClient)
        {
            _dbClient = dbClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var result = _dbClient.Query<int>("select 3 + 3").Single();

            var canConnect = result == 6;
            
            return Ok(new DbPingResponseDto
            {
                CanConnect = canConnect,
            });
        }
    }

    public class DbPingResponseDto
    {
        public bool CanConnect { get; set; }
    }
}

