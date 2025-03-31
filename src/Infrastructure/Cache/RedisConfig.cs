
namespace Infrastructure.Cache;

public class RedisConfig
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = "redis:6379";
    public string InstanceName { get; set; } = "DefaultInstance_";
    public int CacheTimeoutMinutes { get; set; } = 30;
}