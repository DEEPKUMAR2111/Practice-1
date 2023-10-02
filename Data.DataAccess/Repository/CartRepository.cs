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
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private CategoryDbContext _db;
        public CartRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

        public int IncrementCartItem(Cart cart, int count)
        {
           cart.Count+=count;
            return cart.Count;
        }
        public int DecrementCartItem(Cart cart, int count)
        {
            cart.Count -= count;
            return cart.Count;
        }
    }
}
