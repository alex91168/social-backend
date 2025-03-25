using social.Data;
using Microsoft.EntityFrameworkCore;
using social.Services;
using Microsoft.AspNetCore.Identity;
using social.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
var builder = WebApplication.CreateBuilder(args);
Env.Load();
#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnMessageReceived = context => 
            {
                context.Token = context.Request.Cookies["AuthLogin"];
                return Task.CompletedTask;
            }
        };
    });


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddDbContext<ApplicationDatabase>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AWSSQLServer"))); //UseSqlServer, UseNpgsql
builder.Services.AddScoped<IApplicationServices, ApplicationServices>();
builder.Services.AddCors( options => {
    options.AddPolicy("CORS",
        policy => {
            policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader();
        });
});
var app = builder.Build();  

// Configure the HTTP request pipeline.

app.UseCors("CORS");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseHttpsRedirection();


app.Run();

