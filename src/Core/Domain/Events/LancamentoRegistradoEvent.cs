namespace Core.Domain.Events
{
    public record LancamentoRegistradoEvent(
        Guid Id,
        decimal Valor,
        string Tipo,
        DateTime Data,
        string? Descricao);
}