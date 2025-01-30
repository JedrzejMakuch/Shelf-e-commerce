namespace Shelf.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }

        IProductRepository ProductRepository { get; }

        ICompanyRepository CompanyRepository { get; }

        void Save();
    }
}
