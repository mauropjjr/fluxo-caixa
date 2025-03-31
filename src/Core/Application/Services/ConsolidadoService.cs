using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Application.Interfaces;
using Polly;
using MongoDB.Driver;


namespace Core.Application.Services;

public class ConsolidadoService : IConsolidadoService
{
    private readonly IRepository<ConsolidadoDiario> _repository;

    public ConsolidadoService(IRepository<ConsolidadoDiario> repository)
    {
        _repository = repository;
    }

    public async Task<ConsolidadoDiario> ObterConsolidadoPorData(DateTime data)
    {
        var dataKey = data.ToString("yyyy-MM-dd");
        var consolidado = await _repository.GetByIdAsync(dataKey);

        if (consolidado == null)
        {
            return new ConsolidadoDiario
            {
                Id = dataKey,
                Data = data.Date,
                TotalCreditos = 0,
                TotalDebitos = 0,
                SaldoDiario = 0
            };
        }

        return consolidado;
    }

    public async Task ProcessarLancamento(Lancamento lancamento)
    {
        var dataKey = lancamento.Data.ToString("yyyy-MM-dd");

        // Implementação com retry para lidar com concorrência
        var policy = Policy
            .Handle<MongoException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)));

        await policy.ExecuteAsync(async () =>
        {
            var consolidado = await _repository.GetByIdAsync(dataKey) ?? new ConsolidadoDiario
            {
                Id = dataKey,
                Data = lancamento.Data.Date,
                TotalCreditos = 0,
                TotalDebitos = 0,
                SaldoDiario = 0
            };

            if (lancamento.Tipo == "CREDITO")
            {
                consolidado.TotalCreditos += lancamento.Valor;
                consolidado.SaldoDiario += lancamento.Valor;
            }
            else
            {
                consolidado.TotalDebitos += lancamento.Valor;
                consolidado.SaldoDiario -= lancamento.Valor;
            }

            await _repository.UpdateAsync(consolidado);
        });
    }
}