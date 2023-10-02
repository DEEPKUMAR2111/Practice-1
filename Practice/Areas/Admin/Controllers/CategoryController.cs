using Data.DataAccess.Data;
using Data.DataAccess.Repository.IRepository;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Practice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _db;
        public CategoryController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<CategoryModel> categories = _db.CategoryRepository.GetAll();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                _db.CategoryRepository.Add(category);
                _db.Save();
                TempData["success"] = "Category added successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Please fill all the field.";
                return View(category);
            }
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Can not found Category with this name.";
                return NotFound();
            }
            CategoryModel? category = _db.CategoryRepository.GetFirstOrDefault(x => x.CategoryId == id);

            if (category == null)
            {
                TempData["error"] = "Can not found Category with this name.";
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {

                _db.CategoryRepository.Update(category);
                _db.Save();
                TempData["success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Please fill all the field.";
                return View(category);
            }
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Can not found Category with this name.";
                return NotFound();
            }
            CategoryModel? category = _db.CategoryRepository.GetFirstOrDefault(x => x.CategoryId == id);

            if (category == null)
            {
                TempData["error"] = "Can not found Category with this name.";
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteById(int? Id)
        {
            CategoryModel? category = _db.CategoryRepository.GetFirstOrDefault(x => x.CategoryId == Id);

            if (category == null)
            {
                TempData["error"] = "Can not found Category with this name.";
                return NotFound();
            }
            _db.CategoryRepository.Remove(category);
            _db.Save();
            TempData["success"] = "Category deleted successfully.";
            return RedirectToAction("Index");
        }

    }
}
