using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly BlogContext _context;
        public ProfileController(BlogContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // LEN ZATIAL
            HttpContext.Session.SetInt32("userId", 3);
            var user = _context.Users.FirstOrDefault(u => u.Id == HttpContext.Session.GetInt32("userId"));
            return View(user);
        }
        [HttpPost]
        public IActionResult UpdateProfile(User user)
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (userInDb == null)
            {
                return RedirectToAction("Index", "Authentication");
            }

            string oldLogin = userInDb.Login;
            string newLogin = user.Login;
            if (newLogin != oldLogin)
            {
                // ak sa zmenil login, tak sa zistí či už taký login neexistuje
                var sameUser = _context.Users
                    .FirstOrDefault(u => u.Login == newLogin);

                if (sameUser != null)
                {
                    TempData["loginExists"] = "true";
                    return RedirectToAction("Index", "Profile");
                }
            }
            if (ModelState.IsValid) // toto uplne nerozumiem
            {
                userInDb.Name = user.Name;
                userInDb.Surname = user.Surname;
                userInDb.Nick = user.Nick;
                userInDb.Login = user.Login;
                _context.SaveChanges();
                return RedirectToAction("Index", "Profile");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
