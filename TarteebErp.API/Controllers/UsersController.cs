using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TarteebErp.Application.DTOs.Auth;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;

namespace TarteebErp.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UsersController(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            Mobile = u.Mobile,
            RoleId = u.RoleId,
            RoleName = u.Role?.RoleName ?? string.Empty,
            IsActive = u.IsActive
        });
        return Ok(userDtos);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUser(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Mobile = user.Mobile,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] SaveUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required");

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return Conflict("Username already exists");

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            return BadRequest("Selected role does not exist");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Mobile = request.Mobile,
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };

        await _userRepository.AddAsync(user);

        return CreatedAtAction(nameof(GetUser), new { username = user.Username }, ToUserDto(user, role.RoleName));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] SaveUserRequest request)
    {
        if (id != request.Id)
            return BadRequest();

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
            return NotFound();

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            return BadRequest("Selected role does not exist");

        existingUser.FullName = request.FullName;
        existingUser.Email = request.Email;
        existingUser.Mobile = request.Mobile;
        existingUser.RoleId = request.RoleId;
        existingUser.IsActive = request.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;
        existingUser.UpdatedBy = GetCurrentUserId();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            existingUser.PasswordHash = HashPassword(request.Password);
        }
        
        await _userRepository.UpdateAsync(existingUser);

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 1;
    }

    private static UserDto ToUserDto(User user, string roleName)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Mobile = user.Mobile,
            RoleId = user.RoleId,
            RoleName = roleName,
            IsActive = user.IsActive
        };
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));
        
        return $"{Convert.ToBase64String(salt)}:{hashed}";
    }
}

public class SaveUserRequest
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}
