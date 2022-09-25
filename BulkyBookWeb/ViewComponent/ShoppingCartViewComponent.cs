using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utility;

namespace BulkyBookWeb.ViewComponent
{
    public class ShoppingCartViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.CartSession) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.CartSession));
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.CartSession,
                        _unitOfWork.ShoppingCartRepository.GetAll(null,u => u.ApplicationUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(SD.CartSession));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
