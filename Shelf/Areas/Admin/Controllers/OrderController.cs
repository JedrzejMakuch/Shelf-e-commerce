using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using Shelf.Models.ViewModels;
using Shelf.Utility;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;

namespace Shelf.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(e => e.Id == orderViewModel.OrderHeader.Id);

            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult DetailsPayNow()
        {
            orderViewModel = new OrderViewModel
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(e => e.Id == orderViewModel.OrderHeader.Id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(e => e.OrderHeader.Id == orderViewModel.OrderHeader.Id, includeProperties: "Product")
            };

            var domain = "https://localhost:7030";

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"/admin/order/PaymentConfirmation?orderHeaderId={orderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"/admin/order/details?orderId={orderViewModel.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            foreach (var item in orderViewModel.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(i => i.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
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
