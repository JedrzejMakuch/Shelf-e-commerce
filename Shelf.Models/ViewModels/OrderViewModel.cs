using Shelf.Models.Models;

namespace Shelf.Models.ViewModels
{
	public class OrderViewModel
	{
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
