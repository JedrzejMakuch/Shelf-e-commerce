using Microsoft.AspNetCore.Mvc.RazorPages;
using Shelf.Razor.Data;
using Shelf.Razor.Models;

namespace Shelf.Razor.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Category> CategoryList { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
            CategoryList = _context.Categories.ToList();
        }
    }
}
