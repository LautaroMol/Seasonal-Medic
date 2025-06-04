using APISeasonalTicket.DTOs;
using APISeasonalTicket.Services;
using Microsoft.AspNetCore.Mvc;

namespace APISeasonalTicket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbonoController : ControllerBase
    {
        private readonly IAbonoService _abonoService;
        public AbonoController (IAbonoService abonoService)
        {
            _abonoService = abonoService;
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
        public async Task<IActionResult> GetAbonoById(int id)
        {
            var abono = await _abonoService.GetAbonoByIdAsync(id);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }
        [HttpGet("/user/{userId}")]
        public async Task<IActionResult> GetAbonoByUserId(int userId)
        {
            var abono = await _abonoService.GetAbonoByUserId(userId);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }
        [HttpPost]
        public async Task<IActionResult> Post(AbonoDto abonoDto)
        {
            var abono = await _abonoService.Post(abonoDto);
            return Ok(abono);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] AbonoDto abonoDto, int id)
        {
            var abono = await _abonoService.Update(abonoDto, id);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }
        [HttpPatch("{id}/debit")]
        public async Task<IActionResult> UpdateDebit(int id, [FromBody] bool debit)
        {
            var abono = await _abonoService.UpdateDebit(id, debit);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var abono = await _abonoService.Delete(id);
            if (abono == null)
                return NotFound();
            return Ok(abono);
        }
    }
}
