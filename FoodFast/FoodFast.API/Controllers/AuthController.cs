using FoodFast.API.Domain.Entities;
using FoodFast.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodFast.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITokenBlacklistService _tokenBlacklist;

    public AuthController(UserManager<ApplicationUser> userManager, 
                          ITokenGenerator tokenGenerator, 
                          ITokenBlacklistService tokenBlacklist)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _tokenBlacklist = tokenBlacklist;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser 
        { 
            UserName = dto.Email, 
            Email = dto.Email, 
            DisplayName = dto.DisplayName 
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "User created" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return BadRequest();
        }

        var passwordCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordCorrect)
        {
            return Unauthorized();
        }

        var token = _tokenGenerator.GenerateToken(user);
        return Ok(new 
        { 
            access_token = token, 
            userId = user.Id, 
            username = user.UserName 
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        string tokenStr = null!;
        if (Request.Headers.TryGetValue("Authorization", out StringValues value))
        {
            var auth = value.FirstOrDefault();
            if (auth!.StartsWith("Bearer "))
            {
                tokenStr = auth.Substring("Bearer ".Length);
            }
        }
        if (string.IsNullOrEmpty(tokenStr))
        {
            tokenStr = Request.Query["access_token"].FirstOrDefault()!;
        }

        if (string.IsNullOrEmpty(tokenStr))
        {
            return BadRequest("token missing");
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenStr);
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
        {
            return BadRequest("token missing jti");
        }

        var now = DateTime.UtcNow;
        var expires = jwt.ValidTo;
        var ttl = expires > now ? expires - now : TimeSpan.FromSeconds(60);

        await _tokenBlacklist.BlacklistAsync(jti, ttl);
        return Ok(new 
        { 
            message = "logged_out" 
        });
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto userToUpdateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Forbid();
        }

        user.DisplayName = userToUpdateDto.DisplayName;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(updateResult.Errors);
        }

        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id is null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(id);
        return Ok(new 
        { 
            id = user!.Id, 
            email = user.Email, 
            name = user.DisplayName 
        });
    }

    public record RegisterDto(string Email, string Password, string DisplayName);
    public record LoginDto(string Email, string Password);
    public record UpdateUserDto(string DisplayName);
}
