using Data.DataAccess.Data;
using Data.DataAccess.Repository.IRepository;
using Data.Models;
using Data.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace Practice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitOfWork _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<ProductModel> products = _db.ProductRepository.GetAll();
            return View(products);
        }

        public IActionResult Upsert(int? Id)
        {

            ProductVM productVM = new()
            {
                Product = new(),
                Categories = _db.CategoryRepository.GetAll().Select(I => new SelectListItem
                {
                    Value = I.CategoryId.ToString(),
                    Text = I.CategoryName,

                }),
            };
            if (Id == null || Id == 0)
            {
                /*  ViewData["AuthorList"] = AuthorList;
                   ViewBag.AuthorList = AuthorList;*/
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.ProductRepository.GetFirstOrDefault(u => u.ProductId == Id);
                return View(productVM);
            }


        }



        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? File)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (File != null)
                {
                    string FileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Product");
                    var extention = Path.GetExtension(File.FileName);

                    if (productVM.Product.ImgUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImgUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var FileStreams = new FileStream(Path.Combine(uploads, FileName + extention), FileMode.Create))
                    {
                        File.CopyTo(FileStreams);
                    }
                    productVM.Product.ImgUrl = @"\Images\Product\" + FileName + extention;
                }
                if (productVM.Product.ProductId == 0 || productVM.Product.ProductId == null)
                {
                    _db.ProductRepository.Add(productVM.Product);
                    _db.Save();
                    TempData["success"] = "Product added successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _db.ProductRepository.Update(productVM.Product);
                    _db.Save();
                    TempData["success"] = "Product updated successfully.";
                    return RedirectToAction("Index");
                }

            }
            else
            {
                productVM.Categories = _db.CategoryRepository.GetAll().Select(I => new SelectListItem
                {
                    Value = I.CategoryId.ToString(),
                    Text = I.CategoryName,

                });
                TempData["error"] = "Please fill all the field.";
                return View(productVM);
            }
        }

      
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var ProductList = _db.ProductRepository.GetAll(includeProperties: "Category");
            return Json(new { data = ProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            ProductModel obj = _db.ProductRepository.GetFirstOrDefault(u => u.ProductId == Id);


            if (obj == null)
            {
                return Json(new { success = false, message = "Error While Deleting !" });
            }


            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImgUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _db.ProductRepository.Remove(obj);
            _db.Save();
            return Json(new { success = true, message = "Product Deleted Successfully." });



        }
            #endregion

        
    }
}
