using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

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
            if (login.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                return RedirectToAction("Index", "Authentication");
            }
            var user = _context.Users
                .FirstOrDefault(u => EF.Functions.Collate(u.Login, "Latin1_General_BIN") == login);

            if (user == null)
            {
                return RedirectToAction("Index", "Authentication");
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return RedirectToAction("Index", "Authentication");
            }

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userNick", user.Nick);
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
            HttpContext.Session.Remove("userNick");
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
        public IActionResult GuestLogin()
        {
            HttpContext.Session.SetInt32("userId", -1);
            HttpContext.Session.SetString("userPhoto", "/images/profileImages/empty-profile-icon.png");
            HttpContext.Session.SetString("userNick", "Guest");
            return RedirectToAction("Index", "Home", new { ownerOnly = false });
        }
        [HttpPost]
        public IActionResult Register(string name, string surname, string nick, string login, string password, DateTime date)
        {
            var user = new User
            {
                Name = name,
                Surname = surname,
                Nick = nick,
                Login = login,
                Password = password,
                BirthDate = date
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

            // hashovanie hesla
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userPhoto", "/images/profileImages/empty-profile-icon.png");

            return RedirectToAction("Index", "Home", new { ownerOnly = false });
        }
    }
}
