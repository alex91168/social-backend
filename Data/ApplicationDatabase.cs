using Microsoft.EntityFrameworkCore;
using social.Models;

namespace social.Data;

public class ApplicationDatabase : DbContext
{
    public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options) : base(options){}
    public DbSet<User> Users { get; set; }
    public DbSet<UserPosts> UserPosts { get; set; }
}