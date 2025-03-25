using System.ComponentModel.DataAnnotations;

namespace social.Models
{
    public class UserPosts
    {
        public int Id { get; set; }
        [Required]
        public string PostMessage { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty!;
        public DateTime DateCreation { get; set; } = DateTime.Now; //UTC Postgree
    }
}