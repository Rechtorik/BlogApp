using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

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
            foreach (var user in users)
            {
                if (user.ImagePath == null) 
                {
                    user.ImagePath = "/images/profileImages/empty-profile-icon.png";
                }
            }
            if (owner.ImagePath == null)
            {
                owner.ImagePath = "/images/profileImages/empty-profile-icon.png";
            }


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
            if (!tags.IsNullOrEmpty())
            {
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
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!HttpContext.Session.GetInt32("userId").HasValue)
            {
                return RedirectToAction("Logout", "Authentication");
            }
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            blog.Body = blog.Body.Replace("<br>", "\r\n");
            return View(blog);
        }

        [HttpPost]
        public IActionResult Edit(Blog blog)
        {
            var existingBlog = _context.Blogs.FirstOrDefault(b => b.Id == blog.Id);
            existingBlog.Title = blog.Title;
            existingBlog.Body = blog.Body.Replace("\r\n", "<br>").Replace("\n", "<br>");
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Delete(int id)
        {
            // vymazanie tagov
            var tags = _context.Tags.Where(t => t.BlogId == id);
            foreach (var tag in tags)
            {
                _context.Tags.Remove(tag);
            }
            // vymazanie komentov
            var comments = _context.Comments.Where(c => c.BlogId == id);
            foreach (var comment in comments)
            {
                _context.Comments.Remove(comment);
            }
            // vymazanie blogu
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
                    DatePosted = b.DatePosted.ToString("dd.MM.yyyy"),
                    User = _context.Users.Where(u => u.Id == b.UserId).FirstOrDefault().Nick,
                    Tags = _context.Tags.Where(t => t.BlogId == b.Id).Select(t => t.Content).ToList()
                })
                .ToList();
                allBlogs.Reverse();
                return Json(allBlogs);
            }
            var user = _context.Users.FirstOrDefault(u => u.Nick == keyword);
            var searchedTag = _context.Tags.FirstOrDefault(t => t.Content == keyword);
            if (user == null && searchedTag == null)
            {
                return Json(new List<Blog>());
            }

            // Vyhľadávanie blogov podľa tagu

            var blogsWithTagIds = _context.Tags
                .Where(t => t.Content == keyword)
                .Select(t => new
                {
                    t.BlogId
                })
                .ToList();

            var blogsWithTag = _context.Blogs
                .Where(b => blogsWithTagIds.Select(t => t.BlogId).Contains(b.Id))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Body,
                    DatePosted = b.DatePosted.ToString("dd.MM.yyyy"),
                    User = _context.Users.Where(u => u.Id == b.UserId).FirstOrDefault().Nick,
                    Tags = _context.Tags.Where(t => t.BlogId == b.Id).Select(t => t.Content).ToList()

                })
                .ToList();

            // Vyhľadávanie blogov podľa usera

            if (user == null)
            {
                return Json(blogsWithTag);
            }
            int userId = user.Id;
            var blogsWithUser = _context.Blogs
                .Where(b => b.UserId == userId)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Body,
                    DatePosted = b.DatePosted.ToString("dd.MM.yyyy"),
                    User = _context.Users.Where(u => u.Id == b.UserId).FirstOrDefault().Nick,
                    Tags = _context.Tags.Where(t => t.BlogId == b.Id).Select(t => t.Content).ToList()
                })
                .ToList();

            if (blogsWithUser.Any() && blogsWithTag.Any())
            {
                var mergedBlogs = blogsWithTag.Concat(blogsWithUser)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Body,
                        b.DatePosted,
                        b.User,
                        b.Tags
                    })
                    .ToList();
                mergedBlogs = mergedBlogs.OrderByDescending(b => b.DatePosted).ToList();
                return Json(mergedBlogs);
            }
            else if (blogsWithUser.Any())
            {
                return Json(blogsWithUser);
            }
            else
            {
                return Json(blogsWithTag);
            }
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

        [HttpPost]
        public IActionResult UpdateTag(int id, string content)
        {
            // Nájdite tag v databáze
            var tag = _context.Tags.FirstOrDefault(t => t.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            if (content == "[none]")
            {
                _context.Tags.Remove(tag);
            } else
            {
                // Aktualizujte obsah tagu
                tag.Content = content;
            }
            _context.SaveChanges();

            // Vráťte úspešnú odpoveď
            return Ok();
        }
    }
}
