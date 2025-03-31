using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Application.Interfaces;
using Core.Application.DTOs;
using MassTransit;
using Core.Domain.Events;
using static MassTransit.ValidationResultExtensions;


namespace Core.Application.Services;

public class LancamentoService : ILancamentoService
{
    private readonly IRepository<Lancamento> _repository;
    private readonly IBus _bus;
    private readonly ICacheService _cache;

    public LancamentoService(IRepository<Lancamento> repository, IBus bus,
        ICacheService cache)
    {
        _repository = repository;
        _bus = bus;
        _cache = cache;
    }

    public async Task<Lancamento> RegistrarLancamento(LancamentoDto dto)
    {
        var lancamento = new Lancamento
        {
            Id = Guid.NewGuid(),
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            Data = DateTime.Now
        };

        await _repository.AddAsync(lancamento);
        await _cache.RemoveAsync("lancamentos");
        await _bus.Publish(new LancamentoRegistradoEvent(
            lancamento.Id,
            lancamento.Valor,
            lancamento.Tipo,
            lancamento.Data,
            lancamento.Descricao));

        return lancamento;
    }

    public async Task<IEnumerable<Lancamento>> ObterLancamentos()
    {
        var cacheKey = $"lancamentos";
        var cachedResult = await _cache.GetAsync<IEnumerable<Lancamento>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }
        var result = await _repository.GetAllAsync();
        await _cache.SetAsync(cacheKey, result);
        return result;
    }
    public async Task<PagedResultDto<Lancamento>> ObterLancamentosPaginados(PaginationDto pagination)
    {
        var cacheKey = $"lancamentos_page_{pagination.Page}_size_{pagination.PageSize}";
        var cachedResult = await _cache.GetAsync<PagedResultDto<Lancamento>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
        pagination.Page,
            pagination.PageSize);

          var result = new PagedResultDto<Lancamento>
          {
              Items = items,
              TotalItems = totalCount,
              Page = pagination.Page,
              PageSize = pagination.PageSize
          };
        await _cache.SetAsync(cacheKey, result);

        return result;
    }
}