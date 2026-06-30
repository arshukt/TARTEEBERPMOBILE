using Dapper;
using System.Data;
using System.Text;
using TarteebErp.Domain.Entities;
using TarteebErp.Domain.Repositories;
using TarteebErp.Infrastructure.Data;

namespace TarteebErp.Infrastructure.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbConnectionFactory _dbConnectionFactory;
    protected abstract string TableName { get; }
    protected abstract string[] SearchableColumns { get; }
    protected abstract string DefaultSortColumn { get; }

    protected RepositoryBase(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = $"SELECT * FROM \"{TableName}\" WHERE \"Id\" = @Id AND \"IsDeleted\" = false";
        return await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var (orderByColumn, sortDescending) = ResolveSort(null, false);
        var orderDirection = sortDescending ? "DESC" : "ASC";
        var sql = $"SELECT * FROM \"{TableName}\" WHERE \"IsDeleted\" = false ORDER BY \"{orderByColumn}\" {orderDirection}";
        return await conn.QueryAsync<T>(sql);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = new StringBuilder($"SELECT * FROM \"{TableName}\" WHERE \"IsDeleted\" = false");

        if (!string.IsNullOrWhiteSpace(searchTerm) && SearchableColumns.Length > 0)
        {
            var searchConditions = string.Join(" OR ", SearchableColumns.Select(col => $"\"{col}\" LIKE @SearchTerm"));
            sql.Append($" AND ({searchConditions})");
        }

        var (orderByColumn, resolvedSortDescending) = ResolveSort(sortBy, sortDescending);
        var orderDirection = resolvedSortDescending ? "DESC" : "ASC";
        sql.Append($" ORDER BY \"{orderByColumn}\" {orderDirection}");
        sql.Append(" LIMIT @PageSize OFFSET @Offset");

        var parameters = new
        {
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize,
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%"
        };

        return await conn.QueryAsync<T>(sql.ToString(), parameters);
    }

    public virtual async Task<int> GetTotalCountAsync(string? searchTerm = null)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = new StringBuilder($"SELECT COUNT(*) FROM \"{TableName}\" WHERE \"IsDeleted\" = false");

        if (!string.IsNullOrWhiteSpace(searchTerm) && SearchableColumns.Length > 0)
        {
            var searchConditions = string.Join(" OR ", SearchableColumns.Select(col => $"\"{col}\" LIKE @SearchTerm"));
            sql.Append($" AND ({searchConditions})");
        }

        var parameters = new { SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%" };
        return await conn.QuerySingleAsync<int>(sql.ToString(), parameters);
    }

    private (string Column, bool Descending) ResolveSort(string? sortBy, bool sortDescending)
    {
        var sortExpression = string.IsNullOrWhiteSpace(sortBy) ? DefaultSortColumn : sortBy;
        var descending = sortDescending;
        var column = ExtractSortColumn(sortExpression, ref descending);

        if (IsSafeIdentifier(column))
            return (column, descending);

        var defaultDescending = false;
        var defaultColumn = ExtractSortColumn(DefaultSortColumn, ref defaultDescending);

        return IsSafeIdentifier(defaultColumn)
            ? (defaultColumn, defaultDescending)
            : ("Id", false);
    }

    private static string ExtractSortColumn(string sortExpression, ref bool descending)
    {
        var column = sortExpression.Trim().Trim('"');

        if (column.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase))
        {
            descending = true;
            column = column[..^5].Trim();
        }
        else if (column.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
        {
            descending = false;
            column = column[..^4].Trim();
        }

        return column.Trim('"');
    }

    private static bool IsSafeIdentifier(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.All(character => char.IsLetterOrDigit(character) || character == '_');
    }

    public abstract Task AddAsync(T entity);

    public abstract Task UpdateAsync(T entity);

    public virtual async Task DeleteAsync(int id)
    {
        using var conn = _dbConnectionFactory.CreateConnection();
        var sql = $"UPDATE \"{TableName}\" SET \"IsDeleted\" = true, \"DeletedAt\" = @DeletedAt WHERE \"Id\" = @Id";
        await conn.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
    }
}
