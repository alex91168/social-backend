using social.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using social.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using social.Services.Response;
#pragma warning disable CS8604 // Possible null reference argument.
namespace social.Services
{
    public class ApplicationServices : IApplicationServices
    {
        private readonly ApplicationDatabase _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        public ApplicationServices(ApplicationDatabase context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<string> Add(User user)
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName || x.Email == user.Email);
            if( userExists != null)
            {
                throw new UnauthorizedAccessException("Usuario ou email ja cadastrado!");
            }
            if(user.Password == null){
                throw new UnauthorizedAccessException("Senha nao pode ser vazia!");
            }
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return("Usuario criado com sucesso!");
        }

        public async Task<LoginResponse> Login(User user)
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if(userExists == null)
            {
                throw new UnauthorizedAccessException("Usuario não encontrado");
            }
            var passwordVerification = _passwordHasher.VerifyHashedPassword(userExists, userExists.Password, user.Password);
            if(passwordVerification == PasswordVerificationResult.Failed){
                throw new UnauthorizedAccessException("Credenciais erradas.");
            }
            var token = JwtToken(userExists);
            return new LoginResponse{
                Token = token, 
                UserId = userExists.Id
            };
        }

        private string JwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> PromoteUser(int id)
        {
            var UserId = await _context.Users.FindAsync(id); 
            
            if(UserId == null){
                throw new UnauthorizedAccessException("Usuario nao encontrado!");
            }
            if(UserId.Role.Equals("Admin")){
                throw new Exception("Usuario já é Administrador!");
            }
            UserId.Role = "Admin";
            await _context.SaveChangesAsync();
            return("Usuario promovido com sucesso!");
        }

        public async Task<IEnumerable<UserPosts>> GetUserPosts() //props int userId
        {
            var posts = await _context.UserPosts
                                //.Where(x => x.UserId == userId)
                                .OrderByDescending(x => x.DateCreation)
                                .ToListAsync();
            return posts;
        }

        public async Task<string> AddPost(UserPosts userPosts)
        {
            if(userPosts == null){
                throw new ArgumentNullException("Post nao pode ser vazio!");
            }
            _context.UserPosts.Add(userPosts);
            await _context.SaveChangesAsync();

            return "Post criado com sucesso!";
        }

        public async Task<string> DeletePost(int postId, int userId)
        {
            if (postId < 0  || userId < 0 ){
                return "Dados não são validos!";
            }
            var findUser = await _context.Users
                                .Include(posts => posts.UserPosts)
                                .FirstOrDefaultAsync(user => user.Id == userId);
            if(findUser == null){
                return "Usuario não encontrado!";
            } 
            
            if (findUser.UserPosts.Any(x => x.Id == postId)){
                var findPost = await _context.UserPosts.FindAsync(postId);
                _context.UserPosts.Remove(findPost);
                await _context.SaveChangesAsync();
                return "Post deletado com sucesso!";
            }
            return "Usuario não é dono do post.";
        }


    }
}
