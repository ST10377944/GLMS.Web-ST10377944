using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ContractManagement.API.DTOs;

namespace ContractManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            System.Diagnostics.Debug.WriteLine("=== API LOGIN ATTEMPT ===");
            System.Diagnostics.Debug.WriteLine($"Username: {request.username}");

            if (request.username == "admin" && request.password == "password")
            {
                var token = GenerateJwtToken();
                System.Diagnostics.Debug.WriteLine($"Token generated: {(string.IsNullOrEmpty(token) ? "FAILED" : "SUCCESS")}");
                return Ok(new { Token = token });
            }

            System.Diagnostics.Debug.WriteLine("Invalid credentials");
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        private string GenerateJwtToken()
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}