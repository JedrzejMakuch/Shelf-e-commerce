using Microsoft.AspNetCore.Mvc;
using ShelfWeb.Data;
using ShelfWeb.Models;

namespace ShelfWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _context.Categories.ToList();
                
            return View(categoryList);
        }
    }
}
