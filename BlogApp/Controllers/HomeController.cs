using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {

        private readonly BlogContext _context;

        public HomeController(BlogContext context)
        {
            _context = context;
        }

        public IActionResult Index(bool? ownerOnly, int page = 1)
        {
            // kontrola Ëi je pouûÌvateæ prihl·sen˝
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }

            // NaËÌtanie vöetk˝ch blogov z datab·zy
            var blogs = _context.Blogs
                .OrderByDescending(b => b.DatePosted)
                .Skip((page - 1) * 10) // strany bud˙ maù 10 blogov
                .Take(10)
                .ToList();

            int totalBlogs = _context.Blogs.Count();
            int totalPages = (int)Math.Ceiling((double)totalBlogs / 10);

            // Odovzd·me blogy + meta˙daje o str·nkovanÌ do pohæadu
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;


            foreach (var blog in blogs)
            {
                blog.Body = blog.Body.Replace("<br>", " ");
            }

            // NaËÌtanie vöetk˝ch userov z datab·zy
            var users = _context.Users.ToList();
            var tags = _context.Tags.ToList();

            if (ownerOnly.HasValue && ownerOnly == true && userId > 0) 
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
