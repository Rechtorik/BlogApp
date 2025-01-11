using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {

        private readonly BlogContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, BlogContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(bool? ownerOnly)
        {
            // kontrola èi je používate¾ prihlásený
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }

            var userNick = _context.Users.FirstOrDefault(u => u.Id == userId).Nick;
            // Naèítanie všetkých blogov z databázy
            var blogs = _context.Blogs.ToList();

            foreach (var blog in blogs)
            {
                blog.Body = blog.Body.Replace("<br>", " ");
            }

            // Naèítanie všetkých userov z databázy
            var users = _context.Users.ToList();
            var tags = _context.Tags.ToList();

            if (ownerOnly.HasValue && ownerOnly == true) 
            {
                blogs = blogs.Where(b => b.UserId == userId).ToList();
            }
            var model = new HomeIndexViewModel
            {
                Blogs = blogs,
                Users = users,
                Tags = tags
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
