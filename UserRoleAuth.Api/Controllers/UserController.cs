using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using UserRoleAuth.Core.Entities;
using UserRoleAuth.Core.DTOs;
using System.Linq;
using System.Threading.Tasks;
using static UserRoleAuth.Core.DTOs.AuthDtos;
using UserRoleAuth.Api;

namespace UserRoleAuth.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
        }

        // GET: api/user
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName
            });

            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.DisplayName,
                Roles = roles
            });
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            user.Email = dto.Email;
            user.UserName = dto.UserName ?? user.UserName;
            user.DisplayName = dto.DisplayName ?? user.DisplayName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["User_Updated"] });
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["User_Deleted"] });
        }

        // POST: api/user/{id}/roles
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(string id, [FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new { message = _localizer["Invalid_Role"] });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName
                });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["Role_Added"] });
        }
    }
}
