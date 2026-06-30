namespace TarteebErp.Domain.Entities;

public class Role : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<RolePermission> Permissions { get; set; } = new();
}
