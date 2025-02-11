using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Utility;

namespace Shelf.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
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
