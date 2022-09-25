using DataAccess.Repository.IRrpository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ModelView;
using System.Linq;
using Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Product.GetAll("Category,CoverType"));
        }
        //Get
        public IActionResult Create()
        {
            ProductViewModel model = new ProductViewModel();
            
            //2 solutions to set up dropdownlist
            //Solution 1
            model.Categories = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
            });
            //solution 2
            model.CoverTypes = new SelectList(_unitOfWork.CoverType.GetAll(), "Id", "Name");
            return View(model);
        }
        [HttpPost]
        public IActionResult Create(ProductViewModel ProductVm)
        {
            if (ModelState.IsValid)
            {
                if(ProductVm.FormFile != null)
                {
                    var fileName = Guid.NewGuid();
                    var fileExt = Path.GetExtension(ProductVm.FormFile.FileName);
                    var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");

                    using(FileStream fileStream = new FileStream(Path.Combine(uploads, fileName + fileExt), FileMode.Create))
                    {
                        ProductVm.FormFile.CopyTo(fileStream);
                    }
                    ProductVm.Product.ImageUrl = "images/products/" + fileName + fileExt;

                }
                _unitOfWork.Product.Add(ProductVm.Product);
                _unitOfWork.Save();
                TempData["success"] = "Item Added Successfully";
                return RedirectToAction("Index");
            }
            ProductVm.Categories = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
            });
            ProductVm.CoverTypes = new SelectList(_unitOfWork.CoverType.GetAll(), "Id", "Name");
            return View(ProductVm);
        }


        //Get
        public IActionResult Edit(int? id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id, "Category");
            if (id == null || id == 0 || product == null)
            {
                return NotFound();
            }
            ProductViewModel productViewModel = (ProductViewModel)product;
            productViewModel.Categories = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
            });
            productViewModel.CoverTypes = new SelectList(_unitOfWork.CoverType.GetAll(), "Id", "Name");
            return View(productViewModel);
        }
        [HttpPost]
        public IActionResult Edit(ProductViewModel ProductVm)
        {
            if (ModelState.IsValid)
            {
                if(ProductVm.Product.ImageUrl != null && ProductVm.FormFile != null)
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, ProductVm.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    if(ProductVm.FormFile != null)
                    {
                        using (FileStream fileStream = new FileStream(oldPath, FileMode.Create))
                        {
                            ProductVm.FormFile.CopyTo(fileStream);
                        }
                    } 
                }
                _unitOfWork.Product.Update(ProductVm.Product);
                _unitOfWork.Save();
                TempData["success"] = "Item Edited Successfully";
                return RedirectToAction("Index");
            }
            return View(ProductVm);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product product = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            if (product == null)
            {
                return Json(false);
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Item Deleted Successfully";
            return Json(true);
        }
    }
}
