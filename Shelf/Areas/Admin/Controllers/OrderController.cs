using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;

namespace Shelf.Web.Areas.Admin.Controllers
{
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
		public IActionResult GetAll()
		{
			List<OrderHeader> orderList = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
			return Json(new { data = orderList });
		}

		#endregion
	}
}
