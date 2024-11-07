using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shelf.Razor.Data;
using Shelf.Razor.Models;

namespace Shelf.Razor.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public Category Category { get; set; }

        public EditModel(ApplicationDbContext context)
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
            if (ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["success"] = "Category edited successfully";
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}
