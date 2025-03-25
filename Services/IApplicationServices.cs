using Microsoft.AspNetCore.Mvc;
using social.Models;
using social.Services.Response;
namespace social.Services;

public interface IApplicationServices 
{
    Task<IEnumerable<User>> GetUsers();
    Task<string> Add(User user);
    Task<LoginResponse> Login(User user);
    Task<string> PromoteUser(int Id);

    
    //Posts
    Task<IEnumerable<UserPosts>> GetUserPosts(); //int userId props
    Task<string> AddPost(UserPosts userPosts);
    Task<string> DeletePost(int postId, int userId);
}
