using Dapper;
using System.Data;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UserRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*, rp.* 
            FROM ""Users"" u
            LEFT JOIN ""Roles"" r ON u.""RoleId"" = r.""Id""
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE u.""Username"" = @Username AND u.""IsDeleted"" = false";
        
        return await MapUserWithRoleAndPermissions(conn, sql, new { Username = username });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*, rp.* 
            FROM ""Users"" u
            LEFT JOIN ""Roles"" r ON u.""RoleId"" = r.""Id""
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE u.""Id"" = @Id AND u.""IsDeleted"" = false";
        
        return await MapUserWithRoleAndPermissions(conn, sql, new { Id = id });
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*, rp.* 
            FROM ""Users"" u
            LEFT JOIN ""Roles"" r ON u.""RoleId"" = r.""Id""
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE u.""RefreshToken"" = @RefreshToken AND u.""IsDeleted"" = false";
        
        return await MapUserWithRoleAndPermissions(conn, sql, new { RefreshToken = refreshToken });
    }

    public async Task AddAsync(User user)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""FullName"", ""Mobile"", ""Email"", ""RoleId"", ""IsActive"", ""CreatedAt"", ""CreatedBy"")
            VALUES (@Username, @PasswordHash, @FullName, @Mobile, @Email, @RoleId, @IsActive, @CreatedAt, @CreatedBy)
            RETURNING ""Id""";
        user.Id = await conn.QuerySingleAsync<int>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ""Users"" 
            SET ""Username"" = @Username, ""PasswordHash"" = @PasswordHash, ""FullName"" = @FullName, 
                ""Mobile"" = @Mobile, ""Email"" = @Email, ""RoleId"" = @RoleId, ""IsActive"" = @IsActive,
                ""RefreshToken"" = @RefreshToken, ""RefreshTokenExpiry"" = @RefreshTokenExpiry,
                ""UpdatedAt"" = @UpdatedAt, ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
        await conn.ExecuteAsync(sql, user);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*, rp.* 
            FROM ""Users"" u
            LEFT JOIN ""Roles"" r ON u.""RoleId"" = r.""Id""
            LEFT JOIN ""RolePermissions"" rp ON r.""Id"" = rp.""RoleId""
            WHERE u.""IsDeleted"" = false";

        var users = new List<User>();
        await conn.QueryAsync<User, Role, RolePermission, User>(sql, (u, r, rp) =>
        {
            var existingUser = users.FirstOrDefault(x => x.Id == u.Id);
            if (existingUser == null)
            {
                u.Role = r;
                users.Add(u);
                existingUser = u;
            }

            if (existingUser.Role != null && rp != null)
            {
                existingUser.Role.Permissions.Add(rp);
            }

            return existingUser;
        });

        return users;
    }

    private async Task<User?> MapUserWithRoleAndPermissions(IDbConnection conn, string sql, object param)
    {
        User? user = null;
        await conn.QueryAsync<User, Role, RolePermission, User>(sql, (u, r, rp) =>
        {
            if (user == null)
            {
                user = u;
                user.Role = r;
            }

            if (user.Role != null && rp != null)
            {
                user.Role.Permissions.Add(rp);
            }

            return u;
        }, param);

        return user;
    }
}
