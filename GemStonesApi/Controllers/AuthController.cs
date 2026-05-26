using GemStonesApi.Interfaces;
using GemStonesApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GemStonesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [EnableRateLimiting("StrictLimit")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterVM viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.RegisterAsync(viewModel);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("StrictLimit")]
        public async Task<IActionResult> Login(
            [FromBody] LoginVM viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.LoginAsync(viewModel);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
    }
}