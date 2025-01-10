﻿using BlogApp.Models;
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
            //blog.Body.Replace("\r\n", "<br>").Replace("\n", "<br>");
            var user = _context.Users.FirstOrDefault(u => u.Id == blog.UserId);
            var vm = new BlogBlogViewModel
            {
                Blog = blog,
                User = user,
            };
            return View(vm);
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Blog viewModel)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (!userId.HasValue)
            {
                userId = 0;
            }
            var blog = new Blog
            {
                Title = viewModel.Title,
                Body = viewModel.Body.Replace("\r\n", "<br>").Replace("\n", "<br>"),
                DatePosted = DateTime.Now,
                UserId = userId.Value
            };

            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            blog.Body = blog.Body.Replace("<br>", "\r\n");
            return View(blog);
        }

        [HttpPost]
        public IActionResult Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                var existingBlog = _context.Blogs.FirstOrDefault(b => b.Id == blog.Id);
                existingBlog.Title = blog.Title;
                existingBlog.Body = blog.Body.Replace("\r\n", "<br>").Replace("\n", "<br>");
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

        [HttpGet]
        public IActionResult GetFilteredBlogs(string keyword)
        {
            if(keyword == "[none]")
            {
                var allBlogs = _context.Blogs
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    Body = b.Body.Replace("<br>", " "),
                    b.DatePosted,
                    User = _context.Users.Where(u => u.Id == b.UserId).FirstOrDefault().Nick

                })
                .ToList();
                allBlogs.Reverse();
                // Vrátime JSON odpoveď
                return Json(allBlogs);
            }
            var user = _context.Users.FirstOrDefault(u => u.Nick == keyword);
            if (user == null)
            {
                return Json(new List<Blog>());
            }
            int userId = user.Id;
            var filteredBlogs = _context.Blogs
                .Where(b => b.UserId == userId)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Body,
                    b.DatePosted,
                    User = _context.Users.Where(u => u.Id == userId).FirstOrDefault().Nick

                })
                .ToList();
            filteredBlogs.Reverse();

            // Vrátime JSON odpoveď
            return Json(filteredBlogs);
        }
    }
}
