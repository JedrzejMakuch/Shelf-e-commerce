using Microsoft.AspNetCore.Mvc;

namespace ShelfWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
