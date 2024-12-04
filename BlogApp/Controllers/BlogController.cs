using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class BlogController : Controller
    {

        private readonly BlogContext _context;
        public BlogController(BlogContext context)
        {
            _context = context;
        }
        public IActionResult Blog(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            return View(blog);
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            return View(blog);
        }

        [HttpPost]
        public IActionResult Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                var existingBlog = _context.Blogs.FirstOrDefault(b => b.Id == blog.Id);
                existingBlog.Title = blog.Title;
                existingBlog.Body = blog.Body;
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(blog);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _context.Blogs
                .Where(b => b.Id == id)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
