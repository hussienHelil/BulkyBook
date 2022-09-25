using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ModelView;
using Stripe.Checkout;
using System.Security.Claims;
using Utility;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            this._unitOfWork = unitOfWork;
            this._emailSender = emailSender;
        }
        public IActionResult Index()
        {
            //get userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;
            ShoppingCartViewModel model = new ShoppingCartViewModel
            {
                CartList = _unitOfWork.ShoppingCartRepository.GetAll("Product", x => x.ApplicationUserId == userId).ToList(),
                OrderHeader = new(),
            };
            
            foreach(var item in model.CartList)
            {
                model.OrderHeader.OrderTotal += item.Count * item.Product.Price;
            }
            return View(model);
        }
        public IActionResult Summary()
        {
            //get userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;
            ShoppingCartViewModel model = new ShoppingCartViewModel
            {
                CartList = _unitOfWork.ShoppingCartRepository.GetAll("Product", x => x.ApplicationUserId == userId).ToList(),
                OrderHeader = new(),
            };

            model.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(
                u => u.Id == claim.Value);

            model.OrderHeader.Name = model.OrderHeader.ApplicationUser.Name;
            model.OrderHeader.PhoneNumber = model.OrderHeader.ApplicationUser.PhoneNumber;
            model.OrderHeader.StreetAddress = model.OrderHeader.ApplicationUser.StreetAddress;
            model.OrderHeader.City = model.OrderHeader.ApplicationUser.City;
            model.OrderHeader.State = model.OrderHeader.ApplicationUser.State;
            model.OrderHeader.PostalCode = model.OrderHeader.ApplicationUser.PostalCode;
            foreach (var item in model.CartList)
            {
                model.OrderHeader.OrderTotal += item.Count * item.Product.Price;
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShoppingCartViewModel model)
        {
            //get user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(x=> x.Id == userId);


            model.CartList = _unitOfWork.ShoppingCartRepository.GetAll("Product", x => x.ApplicationUserId == userId).ToList();
            model.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(
                u => u.Id == userId);
            model.OrderHeader.OrderDate = System.DateTime.Now;
            model.OrderHeader.ApplicationUserId = model.OrderHeader.ApplicationUserId;
            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                model.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                model.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                model.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                model.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            foreach (var item in model.CartList)
            {
                model.OrderHeader.OrderTotal += item.Count * item.Product.Price;
            }
            _unitOfWork.OrderHeaderRepository.Add(model.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in model.CartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = model.OrderHeader.Id,
                    Price = cart.Product.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }
            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //stripe settings 
                var domain = "https://localhost:44377/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={model.OrderHeader.Id}",
                    CancelUrl = domain + "customer/Cart/Index",
                };

                foreach (var product in model.CartList)
                {

                    SessionLineItemOptions item = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(product.Product.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = product.Product.Title,
                            },

                        },
                        Quantity = product.Count,

                    };
                    options.LineItems.Add(item);


                }


                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(model.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", new { id = model.OrderHeader.Id });
            }
            
        }
        public IActionResult OrderConfirmation(int id)
        {
            var model = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(x => x.Id == id,"ApplicationUser");
            if(model.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(model.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(model.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            _emailSender.SendEmailAsync(model.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created</p>");
            HttpContext.Session.Clear();
            var shoppingList = _unitOfWork.ShoppingCartRepository.GetAll("Product",x => x.ApplicationUserId == model.ApplicationUserId);
            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingList);
            _unitOfWork.Save();
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(x => x.Id == cartId);
            if(shoppingCart != null)
            {
                _unitOfWork.ShoppingCartRepository.IncrementCount(shoppingCart, 1);
                _unitOfWork.Save();
            }
           
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(x => x.Id == cartId);
            if (shoppingCart != null)
            {
                if(shoppingCart.Count <= 1)
                {
                    _unitOfWork.ShoppingCartRepository.Remove(shoppingCart);
                    HttpContext.Session.SetInt32(Utility.SD.CartSession, _unitOfWork.ShoppingCartRepository.GetAll(null, x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count() - 1);
                }
                else
                {
                    _unitOfWork.ShoppingCartRepository.DecrementCount(shoppingCart, 1);

                }
                _unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }

        public IActionResult remove(int cartId)
        {
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(x => x.Id == cartId);
            if (shoppingCart != null)
            {
                _unitOfWork.ShoppingCartRepository.Remove(shoppingCart);
                HttpContext.Session.SetInt32(Utility.SD.CartSession, _unitOfWork.ShoppingCartRepository.GetAll(null, x => x.ApplicationUserId == shoppingCart.ApplicationUserId).Count() -1);
                _unitOfWork.Save();
                
            }

            return RedirectToAction("Index");
        }
    }
}
