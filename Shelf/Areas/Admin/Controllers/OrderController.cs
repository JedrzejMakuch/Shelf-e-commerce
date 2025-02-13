using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Models.ViewModels;
using Shelf.Utility;

namespace Shelf.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderList = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();

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
