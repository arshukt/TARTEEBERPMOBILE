using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;

namespace TarteebErp.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    public RolesController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        var roles = await _roleRepository.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
    {
        role.Id = await _roleRepository.AddAsync(role);
        
        if (role.Permissions != null && role.Permissions.Any())
        {
            await _roleRepository.UpdatePermissionsAsync(role.Id, role.Permissions.Select(p => p.PermissionKey));
        }

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
    {
        if (id != role.Id)
            return BadRequest();

        var existingRole = await _roleRepository.GetByIdAsync(id);
        if (existingRole == null)
            return NotFound();

        await _roleRepository.UpdateAsync(role);
        
        if (role.Permissions != null)
        {
            await _roleRepository.UpdatePermissionsAsync(id, role.Permissions.Select(p => p.PermissionKey));
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        await _roleRepository.DeleteAsync(id);
        return NoContent();
    }
}
