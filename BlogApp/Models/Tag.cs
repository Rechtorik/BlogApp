namespace BlogApp.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public int BlogId { get; set; }
    }
}
