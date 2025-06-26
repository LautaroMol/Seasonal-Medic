using APISeasonalMedic.DTOs;
using APISeasonalMedic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APISeasonalMedic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class AbonoController : ControllerBase
    {
        private readonly IAbonoService _abonoService;

        public AbonoController(IAbonoService abonoService)
        {
            _abonoService = abonoService;
        }

        // Método auxiliar para obtener el UserId del JWT
        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value ??
                             User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Token inválido o userId no encontrado");
            }

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var abonos = await _abonoService.GetAll();
            if (abonos == null)
                return NotFound();
            return Ok(abonos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAbonoById(Guid id)
        {
            var abono = await _abonoService.GetAbonoByIdAsync(id);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAbonoByUserId()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var abono = await _abonoService.GetAbonoByUserId(userId);
                if (abono == null)
                    return NotFound();
                return Ok(abono);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("por-dni/{dni}")]
        public async Task<IActionResult> GetAbonoByDni(string dni)
        {
            var abono = await _abonoService.GetAbonoByDniAsync(dni);
            if (abono == null)
                return NotFound();

            return Ok(abono);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateAbonoDto abonoDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var abono = await _abonoService.Post(abonoDto, userId);
                return Ok(abono);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAbonoDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var existing = await _abonoService.GetAbonoByIdAsync(dto.Id);
                if (existing.UserId != userId)
                    return Forbid();

                var updated = await _abonoService.Update(dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{id}/debit")]
        public async Task<IActionResult> UpdateDebit(Guid id, [FromBody] bool debit)
        {
            try
            {
                var userId = GetUserIdFromToken();
                // Verificar que el abono pertenece al usuario autenticado
                var existingAbono = await _abonoService.GetAbonoByIdAsync(id);
                if (existingAbono?.UserId != userId)
                {
                    return Forbid("No tienes permisos para modificar este abono");
                }

                var abono = await _abonoService.UpdateDebit(id, debit);
                if (abono == null)
                    return NotFound();
                return Ok(abono);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var existing = await _abonoService.GetAbonoByIdAsync(id);
                if (existing.UserId != userId)
                    return Forbid();

                await _abonoService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        [HttpPost("transferir")]
        [Authorize]
        public async Task<IActionResult> Transferir([FromBody] TransferAbonoDto dto)
        {
            var fromUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _abonoService.TransferirAsync(fromUserId, dto.ToUserId, dto.Monto);
            return Ok(new { message = "Transferencia realizada con éxito" });
        }

    }
}