using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UserRoleAuth.Core.Entities;
using UserRoleAuth.Core.DTOs;
using Microsoft.Extensions.Localization;
using static UserRoleAuth.Core.DTOs.AuthDtos;

namespace UserRoleAuth.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<UserController> _localizer;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IStringLocalizer<UserController> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll() =>
            Ok(_userManager.Users.Select(u => new { u.Id, u.UserName, u.Email, u.DisplayName }));


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            var roles = await _userManager.GetRolesAsync(u);

            return Ok(new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName,
                Roles = roles
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateUserDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            u.Email = dto.Email;
            u.DisplayName = dto.DisplayName ?? u.DisplayName;

            var res = await _userManager.UpdateAsync(u);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            return Ok(new { message = _localizer["User_Updated"] });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            var result = await _userManager.DeleteAsync(u);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["User_Deleted"] });
        }


        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(string id, [FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new { message = _localizer["Invalid_Role"] });

            var u = await _userManager.FindByIdAsync(id);
            if (u == null)
                return NotFound(new { message = _localizer["User_Not_Found"] });

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            var res = await _userManager.AddToRoleAsync(u, roleName);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            return Ok(new { message = _localizer["Role_Added"] });
        }
    }
}
