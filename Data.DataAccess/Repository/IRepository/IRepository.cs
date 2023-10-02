using System.Linq.Expressions;

namespace Data.DataAccess.Repository.IRepository
{
    public interface IRepository<T>where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate=null, string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        void Add(T item);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> item);

    }
}
 