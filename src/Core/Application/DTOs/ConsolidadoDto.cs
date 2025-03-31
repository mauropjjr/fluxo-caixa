namespace Core.Application.DTOs;

public class ConsolidadoDto
{
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal SaldoDiario { get; set; }
}