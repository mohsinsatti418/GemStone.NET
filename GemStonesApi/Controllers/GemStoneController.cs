using GemStonesApi.Interfaces;
using GemStonesApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GemStonesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GemStoneController : ControllerBase
    {
        private readonly IGemStoneService _service;

        public GemStoneController(IGemStoneService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a entry of gemstone in the database.
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]        // NEW — Admin only
        public async Task<IActionResult> Create(
            [FromForm] GemStoneCreateVM viewModel,
            [FromForm] List<IFormFile> images,
            [FromForm] int thumbnailIndex = 0)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = await _service.CreateGemStoneAsync(
                viewModel, images, thumbnailIndex);

            return Ok(new
            {
                id = newId,
                message = "GemStone created successfully"
            });
        }

        /// <summary>
        /// Gets all gemstones from the database.
        /// </summary>
        [HttpGet("getall")]
        // No [Authorize] — everyone can browse
        public async Task<IActionResult> GetAll()
        {
            var gemStones = await _service.GetAllGemStonesAsync();
            return Ok(gemStones);
        }

        [HttpGet("getbyid/{id}")]
        // No [Authorize] — everyone can view details
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            var gemStone = await _service.GetGemStoneByIdAsync(id);

            if (gemStone == null)
                return NotFound(new
                {
                    message = $"GemStone with Id {id} was not found"
                });

            return Ok(gemStone);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]        // NEW — Admin only
        public async Task<IActionResult> Update(
            [FromForm] GemStoneUpdateVM viewModel,
            [FromForm] List<IFormFile> newImages)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.UpdateGemStoneAsync(viewModel, newImages);

            return Ok(new { message = "GemStone updated successfully" });
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]        // NEW — Admin only
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            await _service.DeleteGemStoneAsync(id);

            return Ok(new { message = "GemStone deleted successfully" });
        }

        [HttpPatch("restore/{id}")]
        [Authorize(Roles = "Admin")]        // NEW — Admin only
        public async Task<IActionResult> Restore(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            await _service.RestoreGemStoneAsync(id);

            return Ok(new { message = "GemStone restored successfully" });
        }
    }
}