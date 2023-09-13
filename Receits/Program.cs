
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using DataAccess.Models;
using Bussieness;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });
        #endregion

#region Database
        builder.Services.AddDbContext<ReceitContext>(options =>
            options.UseSqlServer("Server=.; Database=Reciets; Trusted_Connection=true; Encrypt=false;"));
#endregion

#region Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ReceitContext>()
    .AddDefaultTokenProviders();

#endregion

#region Authentication
builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "row"; // For Authentication
            options.DefaultChallengeScheme = "row"; //To Handle Challenge
        })
    .AddJwtBearer("row", options =>
    {
        //Use this key when validating requests
        var keyString = builder.Configuration.GetValue<string>("SecretKey");
        var keyInBytes = Encoding.ASCII.GetBytes(keyString!);
        var key = new SymmetricSecurityKey(keyInBytes);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
#endregion

#region MyRegion

#endregion


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();