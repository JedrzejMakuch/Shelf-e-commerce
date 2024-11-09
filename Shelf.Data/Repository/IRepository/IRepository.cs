using System.Linq.Expressions;

namespace Shelf.Data.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetFirstOrDefault(Expression<Func<T, bool>> filer);
        void Add (T entity);
        void Delete (T entity);
        void DeleteRange (IEnumerable<T> entities);
    }
}
