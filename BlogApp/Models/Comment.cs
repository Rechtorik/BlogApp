namespace BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime DatePosted { get; set; }
        public int UserId { get; set; }
        public int BlogId { get; set; }
    }
}
