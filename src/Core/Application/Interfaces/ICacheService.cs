﻿
namespace Core.Application.Interfaces;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T data);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}
