using System.ComponentModel.DataAnnotations;

namespace social.Models;

public class User
{
    public int? Id { get; set; } 
    [Required]
    public string? UserName { get; set;}
    [Required]
    public string? Password { get; set;}
    public string? Email { get; set;}
    public string Role { get; set; } = "User";

    public List<UserPosts> UserPosts { get; set; } = new List<UserPosts>();
    
}

//Colocar para receber imagem de usuario/avatar
