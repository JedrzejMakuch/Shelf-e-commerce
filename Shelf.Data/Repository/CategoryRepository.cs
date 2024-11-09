using Shelf.Data.Data;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Shelf.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) :base(context)
        {
            _context = context;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            _context.Update(category);
        }
    }
}
