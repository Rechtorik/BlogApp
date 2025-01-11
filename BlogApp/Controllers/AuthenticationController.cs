using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var user = _context.Users
                .FirstOrDefault(u => EF.Functions.Collate(u.Login, "Latin1_General_BIN") == login
                         && EF.Functions.Collate(u.Password, "Latin1_General_BIN") == password);

            if (user == null)
            {
                return RedirectToAction("Index", "Authentication");
            }
            HttpContext.Session.SetInt32("userId", user.Id);
            if (user.ImagePath == null)
            {
                HttpContext.Session.SetString("userPhoto", "/images/profileImages/empty-profile-icon.png");
            }
            else
            {
                HttpContext.Session.SetString("userPhoto", user.ImagePath);
            }
            return RedirectToAction("Index", "Home", new { ownerOnly = false });
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userId");
            HttpContext.Session.Remove("userPhoto");
            return RedirectToAction("Index", "Authentication");
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(string name, string surname, string nick, string login, string password)
        {
            var user = new User
            {
                Name = name,
                Surname = surname,
                Nick = nick,
                Login = login,
                Password = password
            };

            if (name == null || surname == null || nick == null || login == null || password == null) 
            {
                TempData["notFilled"] = "true"; // stačí aby tam niečo bolo, nezáleží na hodnote
                return View(user);
            }

            var sameUser = _context.Users.FirstOrDefault(u => u.Login == login);

            if (sameUser != null)
            {
                TempData["takenLogin"] = "true"; // stačí aby tam niečo bolo, nezáleží na hodnote
                //return RedirectToAction("Register", "Authentication");
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userPhoto", "/images/profileImages/empty-profile-icon.png");

            return RedirectToAction("Index", "Home", new { ownerOnly = false });
        }
    }
}
