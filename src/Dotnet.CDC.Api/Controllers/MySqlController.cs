using Dotnet.CDC.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dotnet.CDC.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MySqlController : ControllerBase
    {
        private readonly ILogger<MySqlController> _logger;
        private readonly IMemoryCache _cache;

        public MySqlController(ILogger<MySqlController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public ActionResult<ProductCacheViewModel> Get(int id)
        {
            if (!_cache.TryGetValue(id, out var product))
            {
                _logger.LogError($"Product Id {id} not found");
                return NotFound($"Product Id {id} not found");
            }

            _logger.LogInformation(JsonSerializer.Serialize(product));

            return (ProductCacheViewModel)product;
        }
    }
}
