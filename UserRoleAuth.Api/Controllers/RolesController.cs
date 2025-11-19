using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRoleAuth.Core.Entities;

namespace UserRoleAuth.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        public RolesController(RoleManager<ApplicationRole> roleManager) => _roleManager = roleManager;

        [HttpGet]
        public IActionResult GetAll() => Ok(_roleManager.Roles.Select(r => new { r.Id, r.Name, r.Description }));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApplicationRole role)
        {
            if (await _roleManager.RoleExistsAsync(role.Name)) return BadRequest("Role exists");
            var res = await _roleManager.CreateAsync(role);
            if (!res.Succeeded) return BadRequest(res.Errors);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var r = await _roleManager.FindByIdAsync(id);
            if (r == null) return NotFound();
            var res = await _roleManager.DeleteAsync(r);
            if (!res.Succeeded) return BadRequest(res.Errors);
            return NoContent();
        }
    }
}
