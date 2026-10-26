using Microsoft.AspNetCore.Mvc;

namespace WebApolice.Api.Infrastructure.Errors
{
    // Classe temporária para testes de model binding
    public class TestInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Nome { get; set; } = string.Empty;
    }

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("internal-error-controller")]
        public IActionResult InternalError() => throw new Exception("private-production-test-marker");

        [HttpPost("bad-request")]
        public IActionResult PostBadRequest([FromBody] TestInput input)
        {
            return Ok(input);
        }
    }
}
