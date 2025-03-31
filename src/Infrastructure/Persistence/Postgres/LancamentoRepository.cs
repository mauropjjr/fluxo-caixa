using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Npgsql;
using Dapper;
using System.Linq.Expressions;


namespace Infrastructure.Persistence.Postgres;

public class LancamentoRepository : IRepository<Lancamento>
{
    private readonly string _connectionString;

    public LancamentoRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS lancamentos (
                    Id UUID PRIMARY KEY,
                    Valor DECIMAL NOT NULL,
                    Tipo VARCHAR(10) NOT NULL,
                    Data TIMESTAMP NOT NULL,
                    Descricao VARCHAR(255) NOT NULL
                )");
    }

    public async Task AddAsync(Lancamento entity)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "INSERT INTO lancamentos (Id, Valor, Tipo, Data, Descricao) VALUES (@Id, @Valor, @Tipo, @Data, @Descricao)",
            entity);
    }

    public async Task<IEnumerable<Lancamento>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Lancamento>("SELECT * FROM lancamentos");
    }
    public async Task<(IEnumerable<Lancamento> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<Lancamento, bool>> filter = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var offset = (pageNumber - 1) * pageSize;

        var countQuery = "SELECT COUNT(*) FROM lancamentos";
        var totalCount = await connection.ExecuteScalarAsync<int>(countQuery);

        var query = "SELECT * FROM lancamentos ORDER BY Data DESC OFFSET @Offset LIMIT @PageSize";
        var items = await connection.QueryAsync<Lancamento>(query, new { Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }
    public Task<Lancamento?> GetByIdAsync(object id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Lancamento entity)
    {
        throw new NotImplementedException();
    }
}