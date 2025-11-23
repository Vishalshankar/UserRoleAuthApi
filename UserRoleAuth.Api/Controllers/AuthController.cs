using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoleAuth.Core.Entities;
using UserRoleAuth.Infrastructure.Data;
using UserRoleAuth.Infrastructure.Services;
using UserRoleAuth.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static UserRoleAuth.Core.DTOs.AuthDtos;
using UserRoleAuth.Api;

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
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            ApplicationDbContext db,
            IConfiguration config,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _db = db;
            _config = config;
            _localizer = localizer;
        }

        // ======================
        //      REGISTER
        // ======================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Core.DTOs.AuthDtos.CreateUserDto dto)
        {
            var exists = await _userManager.FindByNameAsync(dto.UserName);
            if (exists != null)
                return BadRequest(_localizer["UsernameExists"]);

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                DisplayName = dto.DisplayName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign default role
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "User",
                    Description = "Default user role"
                });
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = _localizer["UserCreated"] });
        }

        // ======================
        //        LOGIN
        // ======================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Core.DTOs.AuthDtos.LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return Unauthorized(_localizer["InvalidCredentials"]);

            var check = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!check)
                return Unauthorized(_localizer["InvalidCredentials"]);

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"))
            });

            await _db.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "120"))
            });
        }

        // ======================
        //    REFRESH TOKEN
        // ======================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);

            if (stored == null || stored.Expires < DateTime.UtcNow)
                return Unauthorized(_localizer["InvalidRefreshToken"]);

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null)
                return Unauthorized(_localizer["InvalidRefreshToken"]);

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            stored.IsRevoked = true;

            _db.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"))
            });

            await _db.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "120"))
            });
        }
    }
}
