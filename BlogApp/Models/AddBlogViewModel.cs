namespace BlogApp.Models
{
    public class AddBlogViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DatePosted { get; set; }
        public int UserId { get; set; }
    }
}
