using Microsoft.AspNetCore.Mvc;
using DataAccess.Repository.IRrpository;
using System.Collections.Generic;
using Models;
using Microsoft.AspNetCore.Authorization;
using Utility;
using System.Security.Claims;
using Models.ModelView;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using SessionService = Stripe.Checkout.SessionService;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderManagementController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderManagementController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> orders;
            if (User.IsInRole(SD.Admin) || User.IsInRole(SD.Employee))
            {
                orders = _unitOfWork.OrderHeaderRepository.GetAll("ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                string userId = claim.Value;
                orders = _unitOfWork.OrderHeaderRepository.GetAll("ApplicationUser",x=> x.ApplicationUserId == userId);
            }
            if (status != "all" && status != null)
            {
                orders = orders.Where(x => x.OrderStatus.ToLower() == status.ToLower());
            }
            return View(orders);
        }
        public IActionResult Details(int orderId)
        {
            OrderViewModel model = new OrderViewModel
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(x => x.Id == orderId, "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetailRepository.GetAll("Product", x => x.OrderId == orderId),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public IActionResult UpdateOrderDetail(OrderViewModel model)
        {
            var orderHEaderFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == model.OrderHeader.Id);
            orderHEaderFromDb.Name = model.OrderHeader.Name;
            orderHEaderFromDb.PhoneNumber = model.OrderHeader.PhoneNumber;
            orderHEaderFromDb.StreetAddress = model.OrderHeader.StreetAddress;
            orderHEaderFromDb.City = model.OrderHeader.City;
            orderHEaderFromDb.State = model.OrderHeader.State;
            orderHEaderFromDb.PostalCode = model.OrderHeader.PostalCode;
            if (model.OrderHeader.Carrier != null)
            {
                orderHEaderFromDb.Carrier = model.OrderHeader.Carrier;
            }
            if (model.OrderHeader.TrackingNumber != null)
            {
                orderHEaderFromDb.TrackingNumber = model.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHEaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details",  new { orderId = orderHEaderFromDb.Id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public IActionResult StartProcessing(OrderViewModel model)
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(model.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            return RedirectToAction("Details", new { orderId = model.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        public IActionResult ShipOrder(OrderViewModel model)
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == model.OrderHeader.Id);
            orderHeader.TrackingNumber = model.OrderHeader.TrackingNumber;
            orderHeader.Carrier = model.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHeader);
            _unitOfWork.Save();
            return RedirectToAction("Details", new { orderId = model.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Admin + "," + SD.Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(OrderViewModel model)
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == model.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", new { orderId = model.OrderHeader.Id });
        }


        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW(OrderViewModel model)
        {
            model.OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == model.OrderHeader.Id, "ApplicationUser");
            model.OrderDetail = _unitOfWork.OrderDetailRepository.GetAll("Product",u => u.OrderId == model.OrderHeader.Id);

            //stripe settings 
            var domain = "https://localhost:44377/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={model.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={model.OrderHeader.Id}",
            };

            foreach (var item in model.OrderDetail)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);

            }

            var service = new SessionService();
            Stripe.Checkout.Session session = service.Create(options);
            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(model.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Stripe.Checkout.Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(orderHeaderid);
        }

    }
}
