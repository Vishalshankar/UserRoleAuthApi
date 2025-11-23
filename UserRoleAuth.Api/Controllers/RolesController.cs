using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UserRoleAuth.Api;
using UserRoleAuth.Core.Entities;

namespace UserRoleAuth.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RolesController(
            RoleManager<ApplicationRole> roleManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _roleManager = roleManager;
            _localizer = localizer;
        }

        // ======================
        //   GET ALL ROLES
        // ======================
        [HttpGet]
        public IActionResult GetAll()
        {
            var roles = _roleManager.Roles
                .Select(r => new { r.Id, r.Name, r.Description })
                .ToList();

            return Ok(roles);
        }

        // ======================
        //     CREATE ROLE
        // ======================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApplicationRole role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(_localizer["RoleNameRequired"]);

            if (await _roleManager.RoleExistsAsync(role.Name))
                return BadRequest(_localizer["RoleAlreadyExists"]);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["RoleCreated"] });
        }

        // ======================
        //     DELETE ROLE
        // ======================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound(_localizer["RoleNotFound"]);

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = _localizer["RoleDeleted"] });
        }
    }
}
