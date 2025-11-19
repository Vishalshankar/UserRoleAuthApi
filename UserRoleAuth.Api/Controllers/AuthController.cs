using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoleAuth.Core.Entities;
using UserRoleAuth.Infrastructure.Data;
using UserRoleAuth.Infrastructure.Services;
using UserRoleAuth.Core.DTOs;
using Microsoft.EntityFrameworkCore;



namespace UserRoleAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            ApplicationDbContext db,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Core.DTOs.AuthDtos.CreateUserDto dto)
        {
            var exists = await _userManager.FindByNameAsync(dto.UserName);
            if (exists != null) return BadRequest("Username already exists");

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                DisplayName = dto.DisplayName
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Optionally assign default role
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User", Description = "Default user role" });

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Core.DTOs.AuthDtos.LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) return Unauthorized("Invalid credentials");

            var check = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!check) return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            _db.RefreshTokens.Add(new UserRoleAuth.Core.Entities.RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7")),
            });
            await _db.SaveChangesAsync();

            return Ok(new Core.DTOs.AuthDtos.TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "120"))
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);
            if (stored == null || stored.Expires < DateTime.UtcNow) return Unauthorized("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null) return Unauthorized("Invalid refresh token");

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefresh = _jwtService.GenerateRefreshToken();

            // revoke old
            stored.IsRevoked = true;

            _db.RefreshTokens.Add(new Core.Entities.RefreshToken
            {
                Token = newRefresh,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"))
            });
            await _db.SaveChangesAsync();

            return Ok(new Core.DTOs.AuthDtos.TokenResponseDto
            {
                AccessToken = newToken,
                RefreshToken = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "120"))
            });
        }
    }
}
