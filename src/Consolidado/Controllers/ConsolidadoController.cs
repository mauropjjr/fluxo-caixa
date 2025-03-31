
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consolidado.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsolidadoController : ControllerBase
    {
        private readonly IConsolidadoService _service;

        public ConsolidadoController(IConsolidadoService service)
        {
            _service = service;
        }

        [HttpGet("{data}")]
        public async Task<IActionResult> GetByDate(DateTime data)
        {
            var consolidado = await _service.ObterConsolidadoPorData(data);
            return Ok(consolidado);
        }
    }
}
