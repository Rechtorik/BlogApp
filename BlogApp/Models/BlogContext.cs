using Microsoft.EntityFrameworkCore;

namespace BlogApp.Models
{
    public class BlogContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public BlogContext(DbContextOptions options) : base(options)
        {

        }
    }
}
