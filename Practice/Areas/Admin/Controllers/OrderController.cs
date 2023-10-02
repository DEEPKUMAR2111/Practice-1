using Data.DataAccess.Repository.IRepository;
using Data.Models;
using Data.Models.ViewModels;
using Data.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Practice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _db;
        public OrderController(IUnitOfWork db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            var orderVM = new OrderVM()
            {
                OrderHeader = _db.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _db.OrderDetailRepository.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        public IActionResult Details(OrderVM OrderVM)
        {
            var orderHeader = _db.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderHeader.Name = OrderVM.OrderHeader.Name;
            orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeader.Address = OrderVM.OrderHeader.Address;
            orderHeader.City = OrderVM.OrderHeader.City;
            orderHeader.State = OrderVM.OrderHeader.State;
            orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _db.OrderHeaderRepository.Update(orderHeader);
            _db.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing(OrderVM OrderVM)
        {
            _db.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatus.StatusInProcess);

            _db.Save();
            TempData["Success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder(OrderVM OrderVM)
        {
            var OrderHeader = _db.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            OrderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            OrderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            OrderHeader.OrderStatus = OrderStatus.StatusShipped;
            OrderHeader.DateOfShipping = DateTime.Now;
            if (OrderHeader.PaymentStatus == PaymentStatus.PaymenStatusDelayedPayment)
            {
                OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _db.OrderHeaderRepository.Update(OrderHeader);

            _db.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult PayNow(OrderVM OrderVM)
        {
            OrderVM.OrderHeader = _db.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _db.OrderDetailRepository.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");


            var domain = "https://localhost:44354/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain +  $"Customer/Cart/OrderSuccess?id={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };
            foreach (var item in OrderVM.OrderDetail)
            {

                var SessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(SessionLineItem);

            }
            var service = new SessionService();
            Session session = service.Create(options);
            _db.OrderHeaderRepository.UpdatePaymentStatus(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _db.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(OrderVM OrderVM)
        {
            var OrderHeader = _db.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if (OrderHeader.PaymentStatus == PaymentStatus.PaymenStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = OrderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _db.OrderHeaderRepository.UpdateStatus(OrderHeader.Id, OrderStatus.StatusCancelled, PaymentStatus.PaymenStatusRefunded);
            }
            else
            {
                _db.OrderHeaderRepository.UpdateStatus(OrderHeader.Id, OrderStatus.StatusCancelled, PaymentStatus.PaymenStatusRejected);
            }

            _db.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }






        #region API CALLS
        [HttpGet]
        public IActionResult GetAllOrder(string status)
        {
            IEnumerable<OrderHeader> OrderList;

            if(User.IsInRole(UserRole.Role_Admin)|| User.IsInRole(UserRole.Role_Employee))
            {
                OrderList = _db.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
               
            }
            else
            {
                var cliamsIdentity = (ClaimsIdentity)User.Identity;
                var claim = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                OrderList = _db.OrderHeaderRepository.GetAll(x=>x.ApplicationUserId==claim.Value,includeProperties: "ApplicationUser");
            
            }
            switch (status)
            {
                case "pending":
                    OrderList = OrderList.Where(u => u.PaymentStatus == PaymentStatus.PaymentStatusPending);
                    break;
                case "inprocess":
                    OrderList = OrderList.Where(u => u.OrderStatus == OrderStatus.StatusInProcess);
                    break;
                case "completed":
                    OrderList = OrderList.Where(u => u.OrderStatus == OrderStatus.StatusShipped);
                    break;
                case "approved":
                    OrderList = OrderList.Where(u => u.OrderStatus == OrderStatus.StatusApproved);
                    break;
                default:

                    break;

            }
            return Json(new { data = OrderList });
        }
    
    #endregion
}
}
