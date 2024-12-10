using Microsoft.AspNetCore.Mvc;
using wink.Services;
using wink.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace wink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController :  ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;
        public AuthController(IConfiguration configuration,AuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login( string email, string password)
        {
            var user = await _authService.getUsersCrededintials(email);
            if (user == null) {
                return NotFound(new {message = "User not found" });
            }

            if (password == user.Password) 
            {
                var token = GenerateJwtToken(email);
                return Ok(new { Token = token });
            }
            return Unauthorized(new {message=  "Invalid password" });
        }


        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User") // Example of adding a role claim
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Set token expiration
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
