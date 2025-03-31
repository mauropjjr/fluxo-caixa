using Core.Domain.Entities;
using Core.Domain.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;


namespace Infrastructure.Persistence.Mongo;

public class ConsolidadoRepository : IRepository<ConsolidadoDiario>
{
    private readonly IMongoCollection<ConsolidadoDiario> _collection;

    public ConsolidadoRepository(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<ConsolidadoDiario>("Consolidados");
        var indexKeysDefinition = Builders<ConsolidadoDiario>.IndexKeys.Ascending(x => x.Id);
      //  var indexOptions = new CreateIndexOptions { Unique = true };
     //   _collection.Indexes.CreateOne(new CreateIndexModel<ConsolidadoDiario>(indexKeysDefinition, indexOptions));
    }

    public async Task AddAsync(ConsolidadoDiario entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity, new InsertOneOptions { BypassDocumentValidation = false });
        }
        catch (MongoWriteException ex) when (ex.WriteError.Code == 11000)
        {
            // Caso de concorrência: outro request já inseriu o documento
            await UpdateAsync(entity);
        }
    }

    public async Task<ConsolidadoDiario?> GetByIdAsync(object id)
    {
        var filter = Builders<ConsolidadoDiario>.Filter.Eq(x => x.Id, id.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConsolidadoDiario>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task UpdateAsync(ConsolidadoDiario entity)
    {
        var filter = Builders<ConsolidadoDiario>.Filter.Eq(x => x.Id, entity.Id);
        var result = await _collection.ReplaceOneAsync(
            filter,
            entity,
            new ReplaceOptions { IsUpsert = true }); // Upsert garante criação se não existir

        if (result.ModifiedCount == 0 && !result.IsAcknowledged)
        {
            throw new Exception("Falha ao persistir no MongoDB");
        }
    }

    public Task<(IEnumerable<ConsolidadoDiario> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<ConsolidadoDiario, bool>> filter = null)
    {
        throw new NotImplementedException();
    }
}