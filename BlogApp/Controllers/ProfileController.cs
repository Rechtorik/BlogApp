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
            //HttpContext.Session.SetInt32("userId", 3);
            var user = _context.Users.FirstOrDefault(u => u.Id == HttpContext.Session.GetInt32("userId"));
            if (user == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }
            if (user.ImagePath == null)
            {
                user.ImagePath = "/images/profileImages/empty-profile-icon.png";
            }
            return View(user);
        }
        [HttpPost]
        public IActionResult UpdateProfile(User user, IFormFile image)
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (userInDb == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }

            // image
            if (image != null && image.Length > 0)
            {
                // vymažem pôvodný obrázok
                if (userInDb.ImagePath != null)
                {
                    FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userInDb.ImagePath.TrimStart('/')));
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }
                }

                // Nastavíme cestu, kam sa má obrázok uložiť, ak bol nahratý
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profileImages");
                var fileExtension = Path.GetExtension(image.FileName); // Získaj príponu súboru (napr. .jpg, .png)
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}"; // Generuje jedinečný názov súboru
                var filePath = Path.Combine(uploads, randomFileName);

                // Uložíme obrázok na disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                userInDb.ImagePath = $"/images/profileImages/{randomFileName}";
                HttpContext.Session.SetString("userPhoto", userInDb.ImagePath);
                _context.SaveChanges();
            }


            // ak sa nič nezmenilo, tak sa nevykoná update
            if (userInDb.Name == user.Name && userInDb.Surname == user.Surname && userInDb.Nick == user.Nick && userInDb.Login == user.Login)
            {
                return RedirectToAction("Index", "Home");
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
            userInDb.Name = user.Name;
            userInDb.Surname = user.Surname;
            userInDb.Nick = user.Nick;
            userInDb.Login = user.Login;
            _context.SaveChanges();
            return RedirectToAction("Index", "Profile");
        }

        public IActionResult DeleteProfile()
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == HttpContext.Session.GetInt32("userId"));
            if (user == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }
            // nevymazem ho z databazy, len mu vymažem login, heslo a fotku
            if (user.ImagePath != null)
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/')));
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            user.Login = null;
            user.Password = null;
            user.ImagePath = null;
            _context.SaveChanges();
            return RedirectToAction("Logout", "Authentication");
        }
    }
}
