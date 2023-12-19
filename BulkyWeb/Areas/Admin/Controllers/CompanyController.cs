using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles =SD.Role_Admin)]

	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			List<Company> objCompany = _unitOfWork.Company.GetAll().ToList();
			return View(objCompany);
		}

		public IActionResult Upsert(int? id)
		{
			if(id ==null || id == 0)
			{
				return View(new Company());
			} else
			{
				Company comanyObj = _unitOfWork.Company.Get(u=> u.Id == id);

				return View(comanyObj);	
			}

		}

		[HttpPost]
		public IActionResult Upsert(Company CompanyObj)
		{
			if (ModelState.IsValid)
			{
				if(CompanyObj.Id == 0)	
				{
					_unitOfWork.Company.Add(CompanyObj);
				}else
				{
					_unitOfWork.Company.Update(CompanyObj);
				}

				_unitOfWork.Save();
				TempData["success"] = "Company created successfully";
				return RedirectToAction("Index");
			}else
			{
				return View(CompanyObj);
			}
		}

		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> objCompany = _unitOfWork.Company.GetAll().ToList();
			return Json(new { data = objCompany });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);

			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });

			}

			_unitOfWork.Company.Remove(productToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Deleted Successful" });
		}
    #endregion
	}
}
