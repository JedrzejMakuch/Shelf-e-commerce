using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Models.ViewModels;
using Shelf.Utility;
using System.Security.Claims;

namespace Shelf.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderViewModel orderViewModel { get; set; }  

        public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            orderViewModel = new OrderViewModel
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(e => e.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(e => e.OrderHeader.Id == orderId, includeProperties: "Product")
            };

            return View(orderViewModel);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(e => e.Id == orderViewModel.OrderHeader.Id);
            orderHeaderFromDb.Name = orderViewModel.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderViewModel.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderViewModel.OrderHeader.City;
            orderHeaderFromDb.State = orderViewModel.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderViewModel.OrderHeader.PostalCode;

            if(!string.IsNullOrEmpty(orderViewModel.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderViewModel.OrderHeader.Carrier;
            }

            if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details updated successfully";

            return RedirectToAction(nameof(Details), new {orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["success"] = "Order Details updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(e => e.Id == orderViewModel.OrderHeader.Id);
            orderHeaderFromDb.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = orderViewModel.OrderHeader.OrderStatus;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment) {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.OrderHeaderRepository.UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusShipped);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.OrderHeader.Id });
        }

        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> orderList; 

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderList = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderList = _unitOfWork.OrderHeaderRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderList = orderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderList = orderList.Where(u => u.OrderStatus == SD.StatusPending);
                    break;
                case "completed":
                    orderList = orderList.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderList = orderList.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderList });
		}

		#endregion
	}
}
