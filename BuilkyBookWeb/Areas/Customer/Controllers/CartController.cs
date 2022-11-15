using BuilkyBook.DataAccess.Repository;
using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using BuilkyBook.Model.ViewModel;
using BuilkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BuilkyBookWeb.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitWork _unitwork;
        [BindProperty]
        public ShopingCartVM ShopingCartVM { get; set; }
        public CartController(IUnitWork unitWork)
        {
            _unitwork = unitWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var clam = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShopingCartVM = new()
            {
                ListCart = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == clam.Value, includeProperties: "product"),
                orderHeader = new()
            };

            foreach (var cart in ShopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                ShopingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);

            }
            return View(ShopingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM = new ShopingCartVM()
            {
                ListCart = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "product"),
                orderHeader = new()
            };
            ShopingCartVM.orderHeader.ApplicationUser = _unitwork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);

            ShopingCartVM.orderHeader.Name = ShopingCartVM.orderHeader.ApplicationUser.Name;
            ShopingCartVM.orderHeader.PhoneNumber = ShopingCartVM.orderHeader.ApplicationUser.PhoneNumber;
            ShopingCartVM.orderHeader.StreetAddress = ShopingCartVM.orderHeader.ApplicationUser.StreetAddress;
            ShopingCartVM.orderHeader.City = ShopingCartVM.orderHeader.ApplicationUser.City;
            ShopingCartVM.orderHeader.State = ShopingCartVM.orderHeader.ApplicationUser.State;
            ShopingCartVM.orderHeader.PostalCode = ShopingCartVM.orderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                ShopingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);
            }
            return View(ShopingCartVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]

        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM.ListCart = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "product");

            ShopingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShopingCartVM.orderHeader.OrderStatus = SD.StatusPending;
            ShopingCartVM.orderHeader.OrderDate = System.DateTime.Now;
            ShopingCartVM.orderHeader.ApplicationUserId = claim.Value;


            foreach (var cart in ShopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                ShopingCartVM.orderHeader.OrderTotal += (cart.Price * cart.count);
            }

            ApplicationUser applicationUser = _unitwork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShopingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShopingCartVM.orderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShopingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShopingCartVM.orderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitwork.OrderHeader.Add(ShopingCartVM.orderHeader);
            _unitwork.Save();

            foreach (var cart in ShopingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShopingCartVM.orderHeader.Id,
                    Price = cart.Price,
                    Count = cart.count
                };
                _unitwork.OrderDetail.Add(orderDetail);
                _unitwork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7252/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShopingCartVM.orderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/index",
                };

                foreach (var item in ShopingCartVM.ListCart)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.product.Title
                            },

                        },
                        Quantity = item.count,
                    };
                    options.LineItems.Add(sessionLineItem);

                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitwork.OrderHeader.UpdateStripePaymentID(ShopingCartVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitwork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = ShopingCartVM.orderHeader.Id });
            }


        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitwork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitwork.OrderHeader.UpdateStripePaymentID(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitwork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitwork.Save();
                }
            }

            List<ShopingCart> shoppingCarts = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId ==
            orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();
            _unitwork.ShopingCart.RemoveRange(shoppingCarts);
            _unitwork.Save();
            return View(id);
        }


        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }


        public IActionResult Plus(int cartId)
        {
            var cart = _unitwork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitwork.ShopingCart.IncrementCount(cart, 1);
            _unitwork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitwork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.count <= 1)
            {
                _unitwork.ShopingCart.Remove(cart);
                var count = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
                //var count = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;

            }
            else
            {
                _unitwork.ShopingCart.DecrementCount(cart, 1);
            }
            _unitwork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitwork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitwork.ShopingCart.Remove(cart);
            _unitwork.Save();
            var count = _unitwork.ShopingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction(nameof(Index));
        }
    }
}
