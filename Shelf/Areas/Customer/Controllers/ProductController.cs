using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Models.ViewModels;

namespace Shelf.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.ProductRepository.GetAll().ToList();
            
            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductViewModel productVM = new ProductViewModel
            {
                CategoryList = _unitOfWork.CategoryRepository
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.ProductRepository.GetFirstOrDefault(c => c.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel product, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Add(product.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            } else
            {
        
                product.CategoryList = _unitOfWork.CategoryRepository
                .GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

                return View(product);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            Product product = _unitOfWork.ProductRepository.GetFirstOrDefault(e => e.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product product = _unitOfWork.ProductRepository.GetFirstOrDefault(e => e.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _unitOfWork.ProductRepository.Delete(product);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";

            return RedirectToAction("Index");
        }
    }
}
