using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Events;
using MassTransit;

namespace Consolidado.Consumers;

public class LancamentoRegistradoConsumer : IConsumer<LancamentoRegistradoEvent>
{
    private readonly IConsolidadoService _service;

    public LancamentoRegistradoConsumer(IConsolidadoService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<LancamentoRegistradoEvent> context)
    {
        var message = context.Message;
        var lancamento = new Lancamento
        {
            Id = message.Id,
            Valor = message.Valor,
            Tipo = message.Tipo,
            Data = message.Data,
            Descricao = message.Descricao
        };

        await _service.ProcessarLancamento(lancamento);
    }
}
