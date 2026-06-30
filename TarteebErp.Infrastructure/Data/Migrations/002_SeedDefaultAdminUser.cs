using FluentMigrator;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(002)]
public class SeedDefaultAdminUser : Migration
{
    public override void Up()
    {
        var passwordHash = HashPassword("Admin@123");
        
        Insert.IntoTable("Users")
            .Row(new
            {
                Username = "admin",
                PasswordHash = passwordHash,
                FullName = "System Administrator",
                Mobile = (string?)null,
                Email = (string?)null,
                Role = 1, // Admin
                IsActive = true,
                RefreshToken = (string?)null,
                RefreshTokenExpiry = (DateTime?)null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1,
                UpdatedAt = (DateTime?)null,
                UpdatedBy = (int?)null,
                IsDeleted = false,
                DeletedAt = (DateTime?)null,
                DeletedBy = (int?)null
            });
    }

    public override void Down()
    {
        Delete.FromTable("Users").Row(new { Username = "admin" });
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
