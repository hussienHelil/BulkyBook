using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Company.GetAll());
        }
        //Get
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company company)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Add(company);
                _unitOfWork.Save();
                TempData["success"] = "Item Added Successfully";
                return RedirectToAction("Index");
            }
            return View(company);
        }


        //Get
        public IActionResult Edit(int? id)
        {
            var company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            if (id == null || id == 0 || company == null)
            {
                return NotFound();
            }
            return View(company);
        }
        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                TempData["success"] = "Item Edited Successfully";
                return RedirectToAction("Index");
            }
            return View(company);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company company = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return Json(false);
            }
            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();
            TempData["success"] = "Item Deleted Successfully";
            return Json(true);
        }
    }
}
