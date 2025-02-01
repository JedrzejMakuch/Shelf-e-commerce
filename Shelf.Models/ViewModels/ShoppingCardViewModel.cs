using Shelf.Models.Models;

namespace Shelf.Models.ViewModels
{
    public class ShoppingCardViewModel
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
