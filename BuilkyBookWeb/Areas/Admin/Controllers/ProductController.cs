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
    public class ProductController : Controller
    {
        private readonly IUnitWork _unitwork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitWork unitwork, IWebHostEnvironment webHostEnvironment)
        {
            _unitwork = unitwork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            return View();
        }


        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _unitwork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.name,
                    Value = u.id.ToString(),
                }),
                CoverTypeList = _unitwork.CoverType.GetAll().Select(u => new SelectListItem
                {
                    Text = u.name,
                    Value = u.id.ToString(),
                }),

            };

            ////Pass data using viewbag and create drop down using selectItem
            //Product product = new Product();
            //IEnumerable<SelectListItem> CategoryList = _unitwork.Category.GetAll().Select(u => new SelectListItem
            //{
            //    Text = u.name,
            //    Value = id.ToString(),
            //});
            //IEnumerable<SelectListItem> CoverTypeList = _unitwork.CoverType.GetAll().Select(u => new SelectListItem
            //{
            //    Text = u.name,
            //    Value = id.ToString(),
            //});
            if (id == 0 || id == null)
            {
                //ViewBag.CategoryList = CategoryList;
                //ViewBag.CoverTypeList = CoverTypeList;
                return View(productVM);
            }
            else
            {
                productVM.product = _unitwork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string rootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(rootPath, @"Images/Products");
                    var extention = Path.GetExtension(file.FileName);

                    if (obj.product.ImageUrl != null)
                    {
                        var oldImgPath = Path.Combine(rootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);
                        }
                    }
                    //copy file
                    using (var fileStram = new FileStream(Path.Combine(uploads, fileName + extention), FileMode.Create))
                    {
                        file.CopyTo(fileStram);
                    };

                    obj.product.ImageUrl = @"Images/Products/" + fileName + extention;
                }

                if (obj.product.Id == 0)
                {
                    _unitwork.Product.Add(obj.product);
                }
                else
                {
                    _unitwork.Product.Update(obj.product);
                }

                _unitwork.Save();
                TempData["success"] = "Product created  successfully";
                return RedirectToAction("index");
            }

            return View(obj);
        }




        //API endpoint
        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitwork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }
        #endregion

        #region
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var Obj = _unitwork.Product.GetFirstOrDefault(u => u.Id == id);
            if (Obj == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            var oldImgPath = Path.Combine(_webHostEnvironment.WebRootPath, Obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImgPath))
            {
                System.IO.File.Delete(oldImgPath);
            }
            _unitwork.Product.Remove(Obj);
            _unitwork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
