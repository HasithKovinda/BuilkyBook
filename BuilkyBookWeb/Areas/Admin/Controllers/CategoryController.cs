
using BuilkyBook.DataAccess.Data;
using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using BuilkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuilkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitWork _unitwork;

        public CategoryController(IUnitWork unitwork)
        {
            _unitwork = unitwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _unitwork.Category.GetAll();
            return View(categoryList);
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.name == obj.displayOrder.ToString())
            {
                ModelState.AddModelError("name", "Name and Display order cannot be same");
            }
            if (ModelState.IsValid)
            {
                _unitwork.Category.Add(obj);
                _unitwork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("index");
            }

            return View(obj);
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryObj = _unitwork.Category.GetFirstOrDefault(u => u.id == id);
            if (categoryObj == null)
            {
                return NotFound();
            }
            return View(categoryObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.name == obj.displayOrder.ToString())
            {
                ModelState.AddModelError("name", "Name and Display order cannot be same");
            }
            if (ModelState.IsValid)
            {
                _unitwork.Category.Update(obj);
                _unitwork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("index");
            }

            return View(obj);
        }



        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryObj = _unitwork.Category.GetFirstOrDefault(u => u.id == id);
            if (categoryObj == null)
            {
                return NotFound();
            }
            return View(categoryObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category obj)
        {
            _unitwork.Category.Remove(obj);
            _unitwork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("index");

        }
    }
}
