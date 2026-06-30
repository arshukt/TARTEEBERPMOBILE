namespace TarteebErp.Domain.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public string PermissionKey { get; set; } = string.Empty;
}
