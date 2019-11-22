using Confluent.Kafka;
using Dotnet.CDC.Api.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;

namespace Dotnet.CDC.Api.Services
{
    public class CacheMySql : ICacheMySql
    {
        private readonly ILogger<CacheMySql> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _kafkaTopic;

        public CacheMySql(IMemoryCache cache, ILogger<CacheMySql> logger, IConfiguration configuration)
        {
            _cache = cache;
            _logger = logger;

            var conf = new ConsumerConfig
            {
                GroupId =
                    $"mysql.mystore.products.{Guid.NewGuid():N}.group.id", //Choose different group id, because we want to read cache topic from the scratch.
                BootstrapServers = configuration.GetSection("kafkaServer").Value,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _kafkaTopic = configuration.GetSection("kafkaTopicMySql").Value;
            _consumer = new ConsumerBuilder<string, string>(conf).Build();
            _consumer.Subscribe(_kafkaTopic);
        }

        public void Consume(bool returnOnLastOffset, out bool finished, CancellationToken cancellationToken)
        {
            finished = false;
            try
            {
                var watermark = _consumer.QueryWatermarkOffsets(
                    new TopicPartition(_kafkaTopic, new Partition(0)), TimeSpan.FromMilliseconds(60000));

                if (returnOnLastOffset && watermark.High.Value == 0)
                {
                    finished = true;
                    return;
                }

                var consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult.Value == null)
                {
                    var item = JsonSerializer.Deserialize<ProductCacheViewModel>(consumeResult.Key);
                    if (!_cache.TryGetValue(item.Id, out _))
                        return;

                    _logger.LogDebug($"remove cache: {consumeResult.Key}");
                    _cache.Remove(item.Id);
                }
                else
                {
                    var item = JsonSerializer.Deserialize<ProductCacheViewModel>(consumeResult.Value);
                    _logger.LogDebug($"new cache: {consumeResult.Value}");
                    _cache.Set(item.Id, item);
                }

                if (returnOnLastOffset && watermark.High.Value - 1 == consumeResult.Offset.Value)
                    finished = true;
            }
            catch (Exception ex)
            {
                //TODO: alert your health check !!!
                _logger.LogError(ex, ex.Message);
                _consumer.Close();
                _consumer.Dispose();
                throw;
            }
        }
    }
}
