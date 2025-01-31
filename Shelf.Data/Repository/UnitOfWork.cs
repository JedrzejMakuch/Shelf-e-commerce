using Shelf.Data.Data;
using Shelf.Data.Repository.IRepository;

namespace Shelf.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        public IShoppingCartRepository ShoppingCartRepository { get; private set; }
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(_context);
            ProductRepository = new ProductRepository(_context);
            CompanyRepository = new CompanyRepository(_context);
            ShoppingCartRepository = new ShoppingCartRepository(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
