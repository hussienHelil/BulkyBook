using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.CoverType.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(coverType);
                _unitOfWork.Save();
                TempData["success"] = "Item Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

            if (id == 0 || id == null || coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }
        [HttpPost]
        public IActionResult Edit(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(coverType);
                _unitOfWork.Save();
                TempData["success"] = "Item Edited Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        //public IActionResult Delete(int? id)
        //{
        //    var coverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

        //    if (id == 0 || id == null || coverType == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(coverType);
        //}
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            CoverType coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);
            if (coverType == null)
            {
                return Json(false);
            }
            _unitOfWork.CoverType.Remove(coverType);
            _unitOfWork.Save();
            TempData["success"] = "Item Deleted Successfully";
            return Json(true);

        }
    }
}
