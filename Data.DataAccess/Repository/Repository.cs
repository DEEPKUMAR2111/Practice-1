using Data.DataAccess.Data;
using Data.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private CategoryDbContext _db;
        private DbSet<T> dbset;

        public Repository(CategoryDbContext db)
        {
            _db = db;
            dbset = _db.Set<T>();
           // _db.Products.Include(p => p.Category).ToList();
        }

        public void Add(T item)
        {
            dbset.Add(item);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate=null, string? includeProperties=null)
        {
            IQueryable<T> query = dbset;
            if(predicate != null)
            {
                query = query.Where(predicate);
            }
            if(includeProperties!=null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includeProperties = null)
        {

            IQueryable<T> query = dbset;
            query = query.Where(predicate);
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T item)
        {
            dbset.Remove(item);
        }

        public void RemoveRange(IEnumerable<T> item)
        {
            dbset.RemoveRange(item);
        }
    }
}
