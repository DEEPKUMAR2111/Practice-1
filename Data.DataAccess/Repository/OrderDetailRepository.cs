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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private CategoryDbContext _db;
        public OrderDetailRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail data)
        {
            _db.OrderDetails.Update(data);
        }
    }
}
