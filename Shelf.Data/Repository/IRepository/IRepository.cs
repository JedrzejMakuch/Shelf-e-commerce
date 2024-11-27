using System.Linq.Expressions;

namespace Shelf.Data.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T, bool>> filer, string? includeProperties = null);
        void Add (T entity);
        void Delete (T entity);
        void DeleteRange (IEnumerable<T> entities);
    }
}
