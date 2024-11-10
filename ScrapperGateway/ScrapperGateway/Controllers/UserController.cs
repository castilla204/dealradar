// UserController.cs
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DataLayer.Models.PostGresModels;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private const string GoogleClientId = "61603823707-6rf0724oo0vff1695gnkcspuudt1c9hv.apps.googleusercontent.com";

    public UserController(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("google-auth")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthDto authDto)
    {
        try
        {
            // Verify the Google ID token
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { GoogleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(authDto.IdToken, settings);

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.Name, user.Email } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Invalid Google token" });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class GoogleAuthDto
{
    public string IdToken { get; set; }
}