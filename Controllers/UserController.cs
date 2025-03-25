using Microsoft.AspNetCore.Mvc;
using social.Services;
using social.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace social.Controllers;
[Controller]
[Route("[controller]")]

public class UserController : Controller{
    private readonly IApplicationServices _context;

    public UserController(IApplicationServices context)
    {
        _context = context;
    }
    //[Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try {
            var users = await _context.GetUsers();
            if (users == null)
            {
                return NotFound("Lista de usuario vazia.");
            }
            return Ok(users);
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> Add([FromBody] User user)
    {
        if(user == null){
            return BadRequest("Dados invalidos!");
        }
        var result = await _context.Add(user);
        return Ok(result);
    }

    [HttpPost("login-user")]
    public async Task<IActionResult> Login([FromBody] User user){
        if(user == null){
            return BadRequest("Dados invalidos!");
        }
        try {
            var tokenAcess  = await _context.Login(user);
            var token = tokenAcess.Token;
            var userId = tokenAcess.UserId;
            Response.Cookies.Append("AuthLogin", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, //Https
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(1)
            });
            Response.Cookies.Append("UserId", userId.ToString()!, new CookieOptions
            {
                HttpOnly = false,   
                Secure = false, //Https
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(1)
            });
            return Ok(new {Message = "Login efetuado com sucesso!", Token = token});
        }
         catch (UnauthorizedAccessException ex2) {
            throw new Exception(ex2.Message);
        }
        catch (Exception ex){
            return BadRequest( new {message = ex.Message});
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("promote/{id}")]
    public async Task<IActionResult> PromoteUser(int id){
        try {
            var result = await _context.PromoteUser(id);
            return Ok(result);
        } catch (UnauthorizedAccessException ex){
            return BadRequest(ex.Message);
        } catch (Exception ex){
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("auth-user")]
    public IActionResult UserAuth()
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        return Ok(new {message = "Autenticado", role = userRole, user = userName});
    }

    [HttpGet("logout-user")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthLogin");
        return Ok(new {message = "Logout efetuado com sucesso!"});
    }
}