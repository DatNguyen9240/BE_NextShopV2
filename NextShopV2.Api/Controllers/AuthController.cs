using Microsoft.AspNetCore.Mvc;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
                return BadRequest("User already exists");

            var user = new User
            {
                Email = email,
                PasswordHash = password // ⚠️ demo thôi, thực tế nên hash
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            if (user == null) return Unauthorized("Invalid credentials");

            return Ok("Login successful");
        }
    }
}
