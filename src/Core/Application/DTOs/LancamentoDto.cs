namespace Core.Application.DTOs;

public class LancamentoDto
{
    public decimal Valor { get; set; }
    public required string Tipo { get; set; }
    public string? Descricao { get; set; }
}