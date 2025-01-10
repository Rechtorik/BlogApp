namespace BlogApp.Models
{
    public class BlogBlogViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public User Owner { get; set; }
        public Blog Blog { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}
