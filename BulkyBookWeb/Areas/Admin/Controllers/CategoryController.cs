using Models;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Category.GetAll());
        }
        //Get
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Item Added Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        //Get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "Item Edited Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }


        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

        //    if (category == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(category);
        //}
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Category category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            if(category == null)
            {
                return Json(false);
            }
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Item Deleted Successfully";
            return Json(true);
        }
    }
}
