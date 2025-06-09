using APISeasonalMedic.DTOs;
using APISeasonalMedic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APISeasonalMedic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultaMedicaController : ControllerBase
    {
        private readonly IConsultaMedicaService _service;
        public ConsultaMedicaController(IConsultaMedicaService service)
        {
            _service = service;
        }

        // Solicitar nueva consulta (usuario autenticado)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ConsultaMedicaDto>> CrearConsulta([FromBody] CreateConsultaMedicaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("No se pudo identificar al usuario");

            var consulta = await _service.CreateAsync(dto, Guid.Parse(userId));
            return Ok(consulta);
        }

        // ✅ Actualizar consulta (solo agentes o admins)
        [HttpPut("{id}")]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<IActionResult> ActualizarConsulta(Guid id, [FromBody] UpdateConsultaMedicaDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
                return NotFound("Consulta no encontrada");

            return NoContent();
        }

        // ✅ Obtener todas (solo agentes o admins)
        [HttpGet]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<ActionResult<List<ConsultaMedicaDto>>> ObtenerTodas()
        {
            var consultas = await _service.GetAllAsync();
            return Ok(consultas);
        }

        // ✅ Buscar por DNI (agentes/admins)
        [HttpGet("buscar-por-dni/{dni}")]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<ActionResult<List<ConsultaMedicaDto>>> BuscarPorDni(string dni)
        {
            var consultas = await _service.GetByDniAsync(dni);
            return Ok(consultas);
        }

        // ✅ Obtener las propias del usuario logueado
        [HttpGet("mis-consultas")]
        [Authorize]
        public async Task<ActionResult<List<ConsultaMedicaDto>>> ObtenerMisConsultas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var consultas = await _service.GetByUserIdAsync(Guid.Parse(userId));
            return Ok(consultas);
        }
    }
}
