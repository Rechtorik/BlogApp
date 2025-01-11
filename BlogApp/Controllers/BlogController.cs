using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
            var owner = _context.Users.FirstOrDefault(u => u.Id == blog.UserId);
            // nájsť userov, ktorí komentovali na tento blog
            var usersIds = _context.Comments
                .Where(c => c.BlogId == id)
                .Select(c => c.UserId)
                .Distinct()
                .ToList();
            var users = _context.Users
                .Where(u => usersIds.Contains(u.Id))
                .ToList();
            var comments = _context.Comments
                .Where(c => c.BlogId == id)
                .OrderByDescending(c => c.DatePosted)
                .ToList();
            var tags = _context.Tags
                .Where(t => t.BlogId == id)
                .ToList();
            ViewBag.Tags = tags;

            var vm = new BlogBlogViewModel
            {
                Blog = blog,
                Users = users,
                Comments = comments,
                Owner = owner
            };
            return View(vm);
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Blog viewModel, string tags)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Logout", "Authentication");
            }
            var blog = new Blog
            {
                Title = viewModel.Title,
                Body = viewModel.Body.Replace("\r\n", "<br>").Replace("\n", "<br>"),
                DatePosted = DateTime.Now,
                UserId = userId.Value
            };

            _context.Blogs.Add(blog);
            _context.SaveChanges();

            char[] delimiters = { ',', ' ' }; // Zoznam znakov, podľa ktorých sa bude deliť
            string[] allTags = tags.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            blog = _context.Blogs.FirstOrDefault(b => b.Id == blog.Id);

            foreach (string oneTag in allTags)
            {
                var tag = new Tag
                {
                    Content = oneTag,
                    BlogId = blog.Id,
                    UserId = blog.UserId
                };

                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();
            }
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
            if (keyword == "[none]")
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

        [HttpPost]
        public async Task<IActionResult> AddComment(string content, int blogId)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            var comment = new Comment
            {
                Body = content,
                DatePosted = DateTime.Now,
                UserId = userId.Value,
                BlogId = blogId
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Blog", "Blog", new { id = blogId });
        }

        [HttpPost]
        public IActionResult DeleteComment(int commentId) 
        {
            var commentInDb = _context.Comments.Where(c => c.Id == commentId).FirstOrDefault();

            if (commentInDb.UserId != HttpContext.Session.GetInt32("userId")) 
            {
                return RedirectToAction("Index", "Home");
            }

            if (commentInDb != null)
            {
                var blogId = commentInDb.BlogId;
                _context.Comments.Remove(commentInDb);
                _context.SaveChangesAsync();
                return RedirectToAction("Blog", "Blog", new { id = blogId });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
