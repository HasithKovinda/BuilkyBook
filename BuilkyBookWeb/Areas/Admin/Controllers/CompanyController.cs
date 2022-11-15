using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using BuilkyBook.Model.ViewModel;
using BuilkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuilkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitWork _unitwork;


        public CompanyController(IUnitWork unitwork)
        {
            _unitwork = unitwork;

        }

        public IActionResult Index()
        {

            return View();
        }


        public IActionResult Upsert(int? id)
        {
            Company company = new Company();

            if (id == 0 || id == null)
            {
                //ViewBag.CategoryList = CategoryList;
                //ViewBag.CoverTypeList = CoverTypeList;
                return View(company);
            }
            else
            {
                company = _unitwork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {

            if (ModelState.IsValid)
            {

                if (obj.Id == 0)
                {
                    _unitwork.Company.Add(obj);
                    TempData["success"] = "Company created  successfully";
                }
                else
                {
                    _unitwork.Company.Update(obj);
                    TempData["success"] = "Company updated  successfully";
                }

                _unitwork.Save();
                return RedirectToAction("index");
            }

            return View(obj);
        }




        //API endpoint
        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitwork.Company.GetAll();
            return Json(new { data = companyList });
        }
        #endregion

        #region
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var Obj = _unitwork.Company.GetFirstOrDefault(u => u.Id == id);
            if (Obj == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }

            _unitwork.Company.Remove(Obj);
            _unitwork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
