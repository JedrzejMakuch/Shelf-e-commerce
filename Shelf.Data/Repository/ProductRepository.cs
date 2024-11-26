using Shelf.Data.Data;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;

namespace Shelf.Data.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productDb = _context.Products.FirstOrDefault(p => p.Id == product.Id);
            if (productDb != null)
            {
                productDb.Title = product.Title;
                productDb.Description = product.Description;
                productDb.CategoryId = product.CategoryId;
                productDb.Price = product.Price;
                productDb.Price50 = product.Price50;
                productDb.Price100 = product.Price100;
                productDb.ISBN = product.ISBN;
                productDb.Author = product.Author;
                productDb.ListPrice = product.ListPrice;
            
                if(product.ImageUrl != null)
                {
                    productDb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
