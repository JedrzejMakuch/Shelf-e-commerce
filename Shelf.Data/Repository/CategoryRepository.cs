﻿using Shelf.Data.Data;
using Shelf.Data.Repository.IRepository;
using Shelf.Models.Models;

namespace Shelf.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) :base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
