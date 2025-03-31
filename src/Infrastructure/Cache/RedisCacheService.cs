using Core.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisConfig _config;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer _redisConnection;

    public RedisCacheService(
        IDistributedCache cache,
        RedisConfig config,
        ILogger<RedisCacheService> logger,
            IConnectionMultiplexer redisConnection)
    {
        _cache = cache;
        _config = config;
        _logger = logger;
        _redisConnection = redisConnection;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(_config.InstanceName + key);
            return string.IsNullOrEmpty(cachedData)
                ? default
                : JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao acessar o Redis");
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(_config.InstanceName + key);
            _logger.LogInformation("Cache removido para a chave: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cache para a chave: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            var endpoints = _redisConnection.GetEndPoints();
            if (endpoints == null || endpoints.Length == 0)
            {
                _logger.LogWarning("Nenhum endpoint Redis encontrado");
                return;
            }

            var server = _redisConnection.GetServer(endpoints.First());
            if (server == null)
            {
                _logger.LogWarning("Servidor Redis não disponível");
                return;
            }

            // Padrão de busca com o prefixo completo
            var pattern = _config.InstanceName + prefix + "*";
            _logger.LogDebug("Buscando chaves com padrão: {Pattern}", pattern);

            // Usando Scan em vez de Keys para melhor performance
            var keys = new List<RedisKey>();
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keys.Add(key);
            }

            if (keys.Count == 0)
            {
                _logger.LogDebug("Nenhuma chave encontrada com o prefixo: {Prefix}", prefix);
                return;
            }

            var database = _redisConnection.GetDatabase();
            var batch = database.CreateBatch();

            // Paraleliza a exclusão para melhor performance
            var deleteTasks = keys.Select(key =>
            {
                _logger.LogDebug("Marcando chave para remoção: {Key}", key);
                return batch.KeyDeleteAsync(key);
            }).ToArray();

            batch.Execute();
            await Task.WhenAll(deleteTasks);

            _logger.LogInformation("Removidas {Count} chaves com prefixo: {Prefix}",
                keys.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cache por prefixo: {Prefix}", prefix);
            throw; // Re-lança a exceção para tratamento superior
        }
    }

    public async Task SetAsync<T>(string key, T data)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(_config.CacheTimeoutMinutes)
            };

            await _cache.SetStringAsync(
                _config.InstanceName + key,
                JsonSerializer.Serialize(data),
                options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gravar no Redis");
        }
    }
}