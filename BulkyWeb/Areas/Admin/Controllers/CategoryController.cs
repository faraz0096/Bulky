using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork) //constructor when object created constructor called

        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            //two ways to fetch category data from db to list
            // var objCategory = _db.Categories.ToList(); 

            List<Category> objCategory = _unitOfWork.Category.GetAll().ToList();
            //All categories received in objCategory variable
            //we pass objCategory var in view to display categories in views
            return View(objCategory);
        }

        //when user click on Create new Category button this action method will call
        //and show the create category view
        //in Index view we create a button in which we define a Create action method
        public IActionResult Create()

        {
            return View();
        }

        [HttpPost]

        //when click on submit button data will store in obj variable of object Category
        //then save the data in db in curly bracket
        public IActionResult Create(Category obj)
        {

            if (obj.Name == obj.DisplayOrder.ToString())
            {
                //name is the key which is assigned in view asp-for="Name"
                //so the error message will displayed under name field on front-end
                ModelState.AddModelError("name", "The Display Order cannot exactly match the Name.");
            }

            /*if(obj.Name != null && obj.Name.ToLower() == "test")
			{
				ModelState.AddModelError("", "Test is an invalid value");
			}*/
            if (ModelState.IsValid)
            {

                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View();

        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //From edit view we pass id to argument then below LINQ code to find requested id category

            Category? categoryFromDb = _unitOfWork.Category.Get(x => x.Id == id);//it works with only PK
                                                                                 //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(x=> x.Id == id);
                                                                                 //Category? categoryFromDb2 = _db.Categories.Where(x=> x.Id==id).FirstOrDefault();	
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Category.Get(x => x.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitOfWork.Category.Get(x => x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
