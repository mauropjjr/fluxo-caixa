namespace Core.Domain.Entities;

public class ConsolidadoDiario
{
    public required string Id { get; set; } // Formato: yyyy-MM-dd
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal SaldoDiario { get; set; }
}
