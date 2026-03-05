using CompraProgramada.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompraProgramada.Api.Controllers {
    
    [ApiController]
    [Route("api/[controller]")]
    public class MotorController : ControllerBase {
        private readonly MotorServ motorService;

        public MotorController (MotorServ motorServiceInfo) {
            motorService = motorServiceInfo;
        }

        [HttpPost("executar-compra")]
        public async Task<IActionResult> Executar() {
            try {
                var caminhoFicheiro = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "cotacoes", "COTAHIST_D27022026.TXT"));                
                var resultado = await motorService.ExecutarMotorAsync(caminhoFicheiro);
                return Ok(new { mensagem = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}