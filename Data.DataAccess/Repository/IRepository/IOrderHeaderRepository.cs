using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader data);
        void UpdateStatus(int id, string OrderStatus, string? PaymentStatus = null);
        void UpdatePaymentStatus(int id, string SessionId, string? PaymentIntentId);
    }
}
