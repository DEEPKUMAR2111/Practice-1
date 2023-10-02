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
    public class ProductRepository : Repository<ProductModel>, IProductRepository
    {
        private CategoryDbContext _db;
        public ProductRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductModel data)
        {
            var dataFromDb = _db.Products.FirstOrDefault(u => u.ProductId == data.ProductId);
            if (dataFromDb != null)
            {
                dataFromDb.Name = data.Name;
                dataFromDb.Discription = data.Discription;
                dataFromDb.Price = data.Price;
                dataFromDb.Category = data.Category;
                dataFromDb.CategoryId = data.CategoryId;
                if (dataFromDb.ImgUrl != null)
                {
                    dataFromDb.ImgUrl = data.ImgUrl;
                }
            }

        }
    }
}
