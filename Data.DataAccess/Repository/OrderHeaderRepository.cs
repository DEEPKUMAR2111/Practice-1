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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private CategoryDbContext _db;
        public OrderHeaderRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader data)
        {
          _db.OrderHeaders.Update(data);
        }

        public void UpdatePaymentStatus(int id, string SessionId, string? PaymentIntentId)
        {
            var orderHeader=_db.OrderHeaders.FirstOrDefault(x=>x.Id== id);
            orderHeader.PaymentIntentId=PaymentIntentId;
            orderHeader.SessionId=SessionId;
            orderHeader.PaymentDate=DateTime.Now;
          
        }

        public void UpdateStatus(int id, string OrderStatus, string? PaymentStatus = null)
        {
          var order = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(order != null)
            {
                order.OrderStatus=OrderStatus;
            }
            if (PaymentStatus != null)
            {
                order.PaymentStatus = PaymentStatus;
            }
        }


    }
}
