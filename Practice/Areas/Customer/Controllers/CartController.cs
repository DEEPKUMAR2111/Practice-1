using Data.DataAccess.Repository.IRepository;
using Data.Models;
using Data.Models.ViewModels;
using Data.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Practice.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private IUnitOfWork _db;
        public CartController(IUnitOfWork db)
        {
            _db = db;
        }

       
        public IActionResult Index()
        {
            var cliamsIdentity = (ClaimsIdentity)User.Identity;
            var claim = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            CartVM cartVM = new()
             {
                ListCart = _db.CartRepository.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product").ToList(),
                OrderHeader=new OrderHeader()
            };

            foreach (var cart in cartVM.ListCart)
            {
                cartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            return View(cartVM);
        }

        public IActionResult Plus(int CartId)
        {
            var Cart = _db.CartRepository.GetFirstOrDefault(u => u.Id == CartId);
            _db.CartRepository.IncrementCartItem(Cart, 1);
            _db.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int CartId)
        {
            var Cart = _db.CartRepository.GetFirstOrDefault(u => u.Id == CartId);

            if (Cart.Count <= 1)
            {
                _db.CartRepository.Remove(Cart);
                var Count = _db.CartRepository.GetAll(u => u.ApplicationUserId == Cart.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32("SessionCart", Count);
            }
            else
            {
                _db.CartRepository.DecrementCartItem(Cart, 1);
            }

            _db.Save();
            return RedirectToAction("Index");
        }
         public IActionResult Remove(int CartId)
        {
            var cart = _db.CartRepository.GetFirstOrDefault(u => u.Id == CartId);
            _db.CartRepository.Remove(cart);
            _db.Save();
            HttpContext.Session.SetInt32("SessionCart", _db.CartRepository.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count);
            return RedirectToAction("Index");
        }
        public IActionResult Summary()
        {
            var cliamsIdentity = (ClaimsIdentity)User.Identity;
            var claim = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            CartVM cartVM = new()
            {
                ListCart = _db.CartRepository.GetAll(x => x.ApplicationUserId == claim.Value,includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            cartVM.OrderHeader.ApplicationUserId = claim.Value;
            cartVM.OrderHeader.ApplicationUser = _db.ApplicationUserRepository.GetFirstOrDefault(x => x.Id == claim.Value);
            cartVM.OrderHeader.Name = cartVM.OrderHeader.ApplicationUser.Name;
            cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.ApplicationUser.PhoneNumber;
            cartVM.OrderHeader.State = cartVM.OrderHeader.ApplicationUser.State;
            cartVM.OrderHeader.City = cartVM.OrderHeader.ApplicationUser.City;
            cartVM.OrderHeader.Address = cartVM.OrderHeader.ApplicationUser.Address;
            cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.ApplicationUser.PinCode.ToString();
            foreach (var cart in cartVM.ListCart)
            {
                cartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            if(cartVM.OrderHeader.OrderTotal <= 0)
            {
                TempData["Error"] = "Please add item to shop.";
                return RedirectToAction("Index", "Home");
            }
            return View(cartVM);
        }


        [HttpPost]
        public ActionResult Summary(CartVM cartVM)
         {

            var cliamsIdentity = (ClaimsIdentity)User.Identity;
            var claim = cliamsIdentity.FindFirst(ClaimTypes.NameIdentifier);
           cartVM.ListCart = _db.CartRepository.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product");
            cartVM.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            cartVM.OrderHeader.PaymentStatus = PaymentStatus.PaymentStatusPending;
            cartVM.OrderHeader.DateOfOrder = DateTime.Now;
            cartVM.OrderHeader.ApplicationUserId = claim.Value;
            foreach (var cart in cartVM.ListCart)
            {
                cartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }

            _db.OrderHeaderRepository.Add(cartVM.OrderHeader);
            _db.Save();
            foreach (var cart in cartVM.ListCart)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderId = cartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Product.Price,
                };
                _db.OrderDetailRepository.Add(orderDetail);
                _db.Save();
            }


            var domain = "https://localhost:44354/";
            var options = new SessionCreateOptions
            {
              
                LineItems = new List<SessionLineItemOptions>(),
              
                Mode = "payment",
                SuccessUrl = domain +$"Customer/Cart/OrderSuccess?id={cartVM.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",

            };

            foreach(var item in cartVM.ListCart)
            {

                var lineItemsOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100) ,
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(lineItemsOptions);

            }
            var service = new SessionService();
            Session session = service.Create(options);

            _db.OrderHeaderRepository.UpdatePaymentStatus(cartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _db.CartRepository.RemoveRange(cartVM.ListCart);
            _db.Save();
            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);

        }

        public ActionResult OrderSuccess(int id)
        {
            var orderHeader= _db.OrderHeaderRepository.GetFirstOrDefault(x=>x.Id==id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);    
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _db.OrderHeaderRepository.UpdatePaymentStatus(id, session.Id, session.PaymentIntentId);
                _db.OrderHeaderRepository.UpdateStatus(id, OrderStatus.StatusApproved, PaymentStatus.PaymenStatusApproved);
            }
            List<Cart> carts=_db.CartRepository.GetAll(x=>x.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
            _db.CartRepository.RemoveRange(carts);
            _db.Save();
            return View(id);
        }

    }
}
