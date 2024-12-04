using System.Reflection.Metadata.Ecma335;

namespace BlogApp.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DatePosted { get; set; }
        public int UserId { get; set; }
    }
}
