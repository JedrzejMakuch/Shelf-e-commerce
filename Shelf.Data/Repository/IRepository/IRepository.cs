﻿using System.Linq.Expressions;

namespace Shelf.Data.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        void Add (T entity);
        void Delete (T entity);
        void DeleteRange (IEnumerable<T> entities);
    }
}
