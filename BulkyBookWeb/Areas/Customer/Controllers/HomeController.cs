﻿using Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.Product.GetAll("Category"));
        }
        public IActionResult Details(int producatId)
        {
            ShoppingCart shoppingCart = new ShoppingCart
            {
                ProductId = producatId,
                Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == producatId, "Category,CoverType"),
                Count = 1,
            };
            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart model)
        {
            //get current user Id(Id is NameIdentifier)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            model.ApplicationUserId = claim.Value;


            //Check If Shopping Cart Already Exist for current user
            var isShoppingCartProductExist = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(x => x.ProductId == model.ProductId && x.ApplicationUserId == model.ApplicationUserId);

            if (isShoppingCartProductExist == null)
            {
                _unitOfWork.ShoppingCartRepository.Add(model);
                HttpContext.Session.SetInt32(Utility.SD.CartSession, _unitOfWork.ShoppingCartRepository.GetAll(null,x=> x.ApplicationUserId == claim.Value).Count() + 1);
            }
            else
            {
                _unitOfWork.ShoppingCartRepository.IncrementCount(isShoppingCartProductExist, model.Count);
                

            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}