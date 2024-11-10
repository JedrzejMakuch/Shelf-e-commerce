using Shelf.Models.Models;

namespace Shelf.Data.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}
