using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Add()
        {
            return View();
        }
    }
}
