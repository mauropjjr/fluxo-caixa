using Core.Application.DTOs;
using Core.Domain.Entities;

namespace Core.Application.Interfaces;

public interface ILancamentoService
{
    Task<Lancamento> RegistrarLancamento(LancamentoDto dto);
    Task<IEnumerable<Lancamento>> ObterLancamentos();
    Task<PagedResultDto<Lancamento>> ObterLancamentosPaginados(PaginationDto pagination);
}