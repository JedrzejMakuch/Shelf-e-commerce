using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shelf.Razor.Data;
using Shelf.Razor.Models;

namespace Shelf.Razor.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public Category Category { get; set; }

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet(int id)
        {
            if (id != 0 || id != null)
            {
                Category = _context.Categories.FirstOrDefault(c => c.Id == id);
            }
        }

        public IActionResult OnPost()
        {
            if (Category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(Category);
            _context.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToPage("Index");
        }
    }
}
