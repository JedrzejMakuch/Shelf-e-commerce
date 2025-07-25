﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Utility;

namespace Shelf.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companyList = _unitOfWork.CompanyRepository.GetAll().ToList();

            return View(companyList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.CompanyRepository.GetFirstOrDefault(c => c.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.CompanyRepository.Add(company);
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(company);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companytList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = companytList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToDelete = _unitOfWork.CompanyRepository.GetFirstOrDefault(p => p.Id == id);
            if(companyToDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }
            
            _unitOfWork.CompanyRepository.Delete(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
