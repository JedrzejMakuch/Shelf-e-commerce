using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Models.ViewModels;
using System.Security.Claims;

namespace Shelf.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCardViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCardViewModel
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(s => s.ApplicationUserId == userId, includeProperties: "Product")
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int shoppingCartId)
        {
            var shoppingCartDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == shoppingCartId);
            shoppingCartDb.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(shoppingCartDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int shoppingCartId)
        {
            var shoppingCartDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == shoppingCartId);
           
            if(shoppingCartDb.Count <= 1) 
            {
                _unitOfWork.ShoppingCartRepository.Delete(shoppingCartDb);
            } else
            {
                shoppingCartDb.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(shoppingCartDb);
            }
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int shoppingCartId)
        {
            var shoppingCartDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == shoppingCartId);

            if (shoppingCartDb == null)
            {
                return BadRequest();
            }

            _unitOfWork.ShoppingCartRepository.Delete(shoppingCartDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if(shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            } else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
