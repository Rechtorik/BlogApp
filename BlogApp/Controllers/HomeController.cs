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
            // kontrola �i je pou��vate� prihl�sen�
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }

            // Na��tanie v�etk�ch blogov z datab�zy
            var blogs = _context.Blogs
                .OrderByDescending(b => b.DatePosted)
                .Skip((page - 1) * 10) // strany bud� ma� 10 blogov
                .Take(10)
                .ToList();

            int totalBlogs = _context.Blogs.Count();
            int totalPages = (int)Math.Ceiling((double)totalBlogs / 10);

            // Odovzd�me blogy + meta�daje o str�nkovan� do poh�adu
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;


            foreach (var blog in blogs)
            {
                blog.Body = blog.Body.Replace("<br>", " ");
            }

            // Na��tanie v�etk�ch userov z datab�zy
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
