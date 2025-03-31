using Core.Application.DTOs;
using Core.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Lancamentos.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class LancamentosController : ControllerBase
{
    private readonly ILancamentoService _service;
    private readonly IValidator<LancamentoDto> _validator;
    public LancamentosController(ILancamentoService service,
            IValidator<LancamentoDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LancamentoDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Errors = validationResult.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        var lancamento = await _service.RegistrarLancamento(dto);
        return Ok(lancamento);
    }

    //[HttpGet]
    //public async Task<IActionResult> Get([FromQuery] PaginationDto pagination)
    //{
    //    var result = await _service.ObterLancamentosPaginados(pagination);
    //    return Ok(result);
    //}
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.ObterLancamentos();
        return Ok(result);
    }
}

