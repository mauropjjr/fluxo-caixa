namespace Core.Domain.Entities;

public class Lancamento
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public required string Tipo { get; set; } // "CREDITO" ou "DEBITO"
    public DateTime Data { get; set; }
    public string? Descricao { get; set; }
}
