using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using social.Services;
using social.Models;
namespace social.Controllers;

[Controller]
[Route("[controller]")]
public class PostsController : Controller
{
    private readonly IApplicationServices _context;

    public PostsController(IApplicationServices context){
        _context = context;
    }

    [HttpGet("post-user-data")]
    //[Authorize]
    public async Task<IActionResult> GetUserPosts() {
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdParse)) {
            //return BadRequest("Ocorreu um erro de vericiação!");
        //}
        var posts = await _context.GetUserPosts();
        return Ok(posts); //return Ok(posts);
    }
    
    [HttpPost("post-user")]
    [Authorize]
    public async Task<IActionResult> AddPost([FromBody] UserPosts userPosts)
    {
       try{
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdParse) || userName == null ) {
                return BadRequest("Post nao pode ser vazio!");
            }
            userPosts.UserId = userIdParse;
            userPosts.UserName = userName;
            userPosts.DateCreation = DateTime.Now; //UTC Postgree
            var post = await _context.AddPost(userPosts);
            return Ok(post);
       }
       catch(Exception ex){
            Console.WriteLine(ex.Message);
            return BadRequest(new {message = ex.Message, statusText = "Ainda não encontrei o erro :D"});
       }
    }

    [HttpPost("delete-post/{postId}")] 
    [Authorize]
    public async Task<IActionResult> DeletePost(int postId)
    {
        try 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdParse)) {
                return BadRequest(new {message = "Ocorreu um erro de vericiação!" });
            }
            var result = await _context.DeletePost(postId, userIdParse);
            if (result == "Post deletado com sucesso!") {
                return Ok(new {message = result});
            }

            return BadRequest(new {message = result});
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message); 
            return BadRequest(new {message = ex.Message});
        }
    }
}