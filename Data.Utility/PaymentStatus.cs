using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Utility
{
    public class PaymentStatus
    {

        public const string PaymentStatusPending = "Pending";
        public const string PaymenStatusApproved = "Approved";
        public const string PaymenStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymenStatusRejected = "Rejected";
        public const string PaymenStatusRefunded = "Refunded";
    }
}
