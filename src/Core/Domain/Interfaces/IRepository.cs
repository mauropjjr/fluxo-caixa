using System.Linq.Expressions;

namespace Core.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
     int pageNumber,
     int pageSize,
     Expression<Func<T, bool>> filter = null);
}