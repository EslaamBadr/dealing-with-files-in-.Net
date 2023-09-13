using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Context;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Bussieness;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace Receits.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{

    private readonly ReceitContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;

    public UserController(IConfiguration configuration, UserManager<User> userManager, ReceitContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }

    #region Register

    [HttpPost]
    [Route("Register")]
    public async Task<ActionResult> RegisterAsAppUser(RegisterDto registerDto)
    {
        var user = new User
        {
            Name = registerDto.Name,
            UserName = registerDto.UserName
        };

        var creationResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!creationResult.Succeeded)
        {
            return BadRequest(creationResult.Errors);
        }

        var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.UserName)
            };

        var addingClaimsResult = await _userManager.AddClaimsAsync(user, claims);
        if (!addingClaimsResult.Succeeded)
        {
            return BadRequest(addingClaimsResult.Errors);
        }

        return NoContent();
    }

    #endregion


    #region login

    [HttpPost]
    [Route("Login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto credentials)
    {
        var user = await _userManager.FindByNameAsync(credentials.UserName);
        if (user == null)
        {
            return BadRequest();
        }

        bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, credentials.Password);
        if (!isPasswordCorrect)
        {
            return BadRequest();
        }

        //List<Claim> claimsList = (await _userManager.GetClaimsAsync(user)).ToList();
        var keyString = _configuration.GetValue<string>("SecretKey");
        var keyInBytes = Encoding.ASCII.GetBytes(keyString!);
        var key = new SymmetricSecurityKey(keyInBytes);

        // Hashing Criteria 
        SigningCredentials signingCredentials = new SigningCredentials(key,
            SecurityAlgorithms.HmacSha256Signature);

        // Putting All together
        DateTime exp = DateTime.Now.AddDays(200);
        JwtSecurityToken token = new JwtSecurityToken(
                //claims: claimsList,
                signingCredentials: signingCredentials,
                expires: exp
            );

        var tokenHandler = new JwtSecurityTokenHandler();
        string tokenString = tokenHandler.WriteToken(token);

        return new TokenDto
        {
            Token = tokenString,
            Expiry = exp,
        };
    }

    #endregion
}
