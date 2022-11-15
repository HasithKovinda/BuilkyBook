
using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using BuilkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BuilkyBookWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		private readonly IUnitWork _unitwork;


		public HomeController(ILogger<HomeController> logger, IUnitWork unitwork)
		{
			_logger = logger;
			_unitwork = unitwork;
		}

		public IActionResult Index()
		{
			IEnumerable<Product> productList = _unitwork.Product.GetAll(includeProperties: "Category,CoverType");
			return View(productList);
		}

		public IActionResult Details(int productId)
		{
			ShopingCart cartObj = new()
			{
				count = 1,
				ProductId = productId,
				product = _unitwork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType"),
			};

			return View(cartObj);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public IActionResult Details(ShopingCart shopingCart)
		{
			//get userId
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var clam = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			shopingCart.ApplicationUserId = clam.Value;

			ShopingCart obj = _unitwork.ShopingCart.GetFirstOrDefault(u => u.ApplicationUserId == clam.Value && u.ProductId == shopingCart.ProductId);
			if (obj == null)
			{
				_unitwork.ShopingCart.Add(shopingCart);
				_unitwork.Save();
				HttpContext.Session.SetInt32(SD.SessionCart, _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == clam.Value).ToList().Count);
			}
			else
			{
				_unitwork.ShopingCart.IncrementCount(obj, obj.count);
				_unitwork.Save();
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