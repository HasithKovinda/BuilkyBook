using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using BuilkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuilkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {

        private readonly IUnitWork _unitwork;

        public CoverTypeController(IUnitWork unitwork)
        {
            _unitwork = unitwork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> categoryList = _unitwork.CoverType.GetAll();
            return View(categoryList);
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {


            if (ModelState.IsValid)
            {
                _unitwork.CoverType.Add(obj);
                _unitwork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("index");
            }

            return View(obj);
        }


        public IActionResult Edit(int? id)
        {

            var categoryObj = _unitwork.CoverType.GetFirstOrDefault(u => u.id == id);
            if (categoryObj == null)
            {
                return NotFound();
            }
            return View(categoryObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (obj.name == obj.id.ToString())
            {
                ModelState.AddModelError("name", "Name and Display order cannot be same");
            }
            if (ModelState.IsValid)
            {
                _unitwork.CoverType.Update(obj);
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
            var categoryObj = _unitwork.CoverType.GetFirstOrDefault(u => u.id == id);
            if (categoryObj == null)
            {
                return NotFound();
            }
            return View(categoryObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CoverType obj)
        {
            _unitwork.CoverType.Remove(obj);
            _unitwork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("index");

        }
    }
}
