using Shelf.Data.Data;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;

namespace Shelf.Data.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) :base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderHeaderDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if(orderHeaderDb != null)
			{
				orderHeaderDb.OrderStatus = orderStatus;
				if(!string.IsNullOrEmpty(paymentStatus))
				{
					orderHeaderDb.PaymentStatus = paymentStatus;
				}
			}
		}

		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderHeaderDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if(!string.IsNullOrEmpty(sessionId))
			{
				orderHeaderDb.SessionId = sessionId;
			}

			if (!string.IsNullOrEmpty(paymentIntentId))
			{
				orderHeaderDb.PaymentIntentId = paymentIntentId;
				orderHeaderDb.PaymentDate = DateTime.Now;
			}
		}
	}
}
