using Data.DataAccess.Repository.IRepository;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Practice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IUnitOfWork _db;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<ProductModel> Products = _db.ProductRepository.GetAll(includeProperties: "Category");
            return View(Products);
        }

        [HttpGet]
        public IActionResult Details(int ProductId)
        {
          var cart = new Cart() 
            {
                Count = 1,
                ProductId = ProductId,
                Product = _db.ProductRepository.GetFirstOrDefault(u => u.ProductId == ProductId, includeProperties: "Category")
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
        
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claims.Value;

            var cartItem=_db.CartRepository.GetFirstOrDefault(x=>x.ProductId==cart.ProductId && x.ApplicationUserId==claims.Value);
            if (cartItem == null)
            {
                _db.CartRepository.Add(cart);
                _db.Save();
           
                    HttpContext.Session.SetInt32("SessionCart", _db.CartRepository.GetAll(u => u.ApplicationUserId == claims.Value).ToList().Count);
                
            }
            else
            {
                _db.CartRepository.IncrementCartItem(cartItem,cart.Count);
                _db.Save();
            }
          
            return RedirectToAction("Index");
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}