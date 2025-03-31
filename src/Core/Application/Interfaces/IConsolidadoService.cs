using Core.Domain.Entities;

namespace Core.Application.Interfaces;

public interface IConsolidadoService
{
    Task<ConsolidadoDiario> ObterConsolidadoPorData(DateTime data);
    Task ProcessarLancamento(Lancamento lancamento);
}