using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class BlogController : Controller
    {

        private readonly BlogContext _context;
        public BlogController(BlogContext context)
        {
            _context = context;
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBlogViewModel viewModel)
        {
            var blog = new Blog
            {
                Title = viewModel.Title,
                Body = viewModel.Body,
                DatePosted = DateTime.Now,
                UserId = 0 // Len zatiaľ
            };

            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
