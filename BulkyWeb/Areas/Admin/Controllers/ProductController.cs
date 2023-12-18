using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;


namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] //you can apply indivisually on each Action methods
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
            List<Product> objProduct = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
           
            return View(objProduct);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {

                CategoryList = _unitOfWork.Category
                 .GetAll().Select(u => new SelectListItem
                 {
                     Text = u.Name,
                     Value = u.Id.ToString()
                 }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                //Create
				return View(productVM);

			} else
            {
				//update
				productVM.Product = _unitOfWork.Product.Get(u=> id == u.Id);
                return View(productVM);
            }

          //  ViewBag.CategoryList = CategoryList;
            
        }

        [HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
           if (ModelState.IsValid)
            {
                string wweRootPath = _webHostEnvironment.WebRootPath;//to get rootpath wwwroot

               
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);//to convert filename to guid
                    string productPath = Path.Combine(wweRootPath, @"images\product");
                
                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wweRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    //Save image
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Category created successfully";
                } else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Category updated successfully";
                }
                _unitOfWork.Save();
				
                return RedirectToAction("Index");
			} else
            {

				productVM.CategoryList = _unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                    
                };
				return View(productVM);
			}
	
      
        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProduct = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProduct });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u=> u.Id == id);

            if(productToBeDeleted == null)
            {
                return Json (new { success = false, message = "Error while deleting" });

            }

            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                    productToBeDeleted.ImageUrl.TrimStart('\\'));


                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successful" });
        }
        #endregion

    }
}
