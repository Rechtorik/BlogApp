using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly BlogContext _context;
        public AuthenticationController(BlogContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Login(string login, string password)
        {
            // Check if the user exists in the database
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            // If the user exists, redirect to the home page
            if (user == null)
            {
                return RedirectToAction("Index", "Authentication");
            }
            // If the user does not exist, return an error message
            // nastaviť session loginId
            HttpContext.Session.SetInt32("userId", user.Id);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userId");
            return RedirectToAction("Index", "Authentication");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
