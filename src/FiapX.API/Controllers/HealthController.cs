using Microsoft.AspNetCore.Mvc;

namespace FiapX.API.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint utilizado para verificação de saúde da API
    /// </summary>
    /// <returns>Status da aplicação</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "fiapx-api",
            timestamp = DateTime.UtcNow
        });
    }
}