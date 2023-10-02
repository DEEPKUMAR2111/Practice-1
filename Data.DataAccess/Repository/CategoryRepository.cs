using Data.DataAccess.Data;
using Data.DataAccess.Repository.IRepository;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DataAccess.Repository
{
    public class CategoryRepository : Repository<CategoryModel>, ICategoryRepository
    {
        private CategoryDbContext _db;
        public CategoryRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CategoryModel category)
        {
          _db.Categories.Update(category);
        }
    }
}
