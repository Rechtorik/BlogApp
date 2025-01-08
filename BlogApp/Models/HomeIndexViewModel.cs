namespace BlogApp.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Blog> Blogs { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}
