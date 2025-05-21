// Controllers/SoloController.cs
using Microsoft.AspNetCore.Mvc;
using ShredleApi.Models;
using ShredleApi.Services;

namespace ShredleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoloController : ControllerBase
    {
        private readonly SoloService _soloService;

        public SoloController(SoloService soloService)
        {
            _soloService = soloService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SoloResponse>> GetSolo(int id)
        {
            var solo = await _soloService.GetSoloByIdAsync(id);
            
            if (solo == null)
            {
                return NotFound($"Solo with ID {id} not found");
            }

            var response = new SoloResponse
            {
                Id = solo.Id,
                Title = solo.Title,
                Artist = solo.Artist,
                SpotifyId = solo.SpotifyId,
                StartTimeClip1 = solo.StartTimeClip1,
                EndTimeClip1 = solo.EndTimeClip1,
                StartTimeClip2 = solo.StartTimeClip2,
                EndTimeClip2 = solo.EndTimeClip2,
                StartTimeClip3 = solo.StartTimeClip3,
                EndTimeClip3 = solo.EndTimeClip3,
                StartTimeClip4 = solo.StartTimeClip4,
                EndTimeClip4 = solo.EndTimeClip4,
                Guitarist = solo.Guitarist,
                Hint = solo.Hint
            };

            return Ok(response);
        }
    }
}