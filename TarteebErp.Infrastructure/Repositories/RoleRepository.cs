using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RoleRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT r.*, rp.* 
            FROM ""Roles"" r
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE r.""IsDeleted"" = false";
        
        var roleDict = new Dictionary<int, Role>();
        
        await conn.QueryAsync<Role, RolePermission, Role>(sql, (r, rp) =>
        {
            if (!roleDict.TryGetValue(r.Id, out var role))
            {
                role = r;
                roleDict.Add(role.Id, role);
            }

            if (rp != null)
            {
                role.Permissions.Add(rp);
            }

            return role;
        });

        return roleDict.Values;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT r.*, rp.* 
            FROM ""Roles"" r
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE r.""Id"" = @Id AND r.""IsDeleted"" = false";
        
        Role? role = null;
        
        await conn.QueryAsync<Role, RolePermission, Role>(sql, (r, rp) =>
        {
            if (role == null)
            {
                role = r;
            }

            if (rp != null)
            {
                role.Permissions.Add(rp);
            }

            return role;
        }, new { Id = id });

        return role;
    }

    public async Task<int> AddAsync(Role role)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Roles"" (""RoleName"", ""Description"", ""IsActive"")
            VALUES (@RoleName, @Description, @IsActive)
            RETURNING ""Id""";
        
        return await conn.QuerySingleAsync<int>(sql, role);
    }

    public async Task UpdateAsync(Role role)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Roles"" 
            SET ""RoleName"" = @RoleName, ""Description"" = @Description, ""IsActive"" = @IsActive
            WHERE ""Id"" = @Id";
        
        await conn.ExecuteAsync(sql, role);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Roles"" 
            SET ""IsDeleted"" = true
            WHERE ""Id"" = @Id";
        
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task UpdatePermissionsAsync(int roleId, IEnumerable<string> permissions)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        
        // Delete all existing permissions
        const string deleteSql = @"DELETE FROM ""RolePermissions"" WHERE ""RoleId"" = @RoleId";
        await conn.ExecuteAsync(deleteSql, new { RoleId = roleId });

        // Insert new permissions
        if (permissions.Any())
        {
            const string insertSql = @"
                INSERT INTO ""RolePermissions"" (""RoleId"", ""PermissionKey"")
                VALUES (@RoleId, @PermissionKey)";
            
            var parameters = permissions.Select(p => new { RoleId = roleId, PermissionKey = p });
            await conn.ExecuteAsync(insertSql, parameters);
        }
    }
}
