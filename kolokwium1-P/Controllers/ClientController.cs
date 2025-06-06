using Microsoft.AspNetCore.Mvc;
using kolokwium1_P.Services;
using kolokwium1_P.Models;

namespace kolokwium1_P.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _service;

        public ClientsController(IClientService service)
        {
            _service = service;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClient(int clientId)
        {
            var result = await _service.GetClientWithRentalsAsync(clientId);

            return result == null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddClientRental([FromBody] ClientRentalRequestsDTo request)
        {
            if (request?.Client?.FirstName == null || request.Client.FirstName.Trim().Length == 0)
                return BadRequest();

            try
            {
                var added = await _service.AddClientWithRentalAsync(request);
                if (added == null)
                    return NotFound("Samochód nie istnieję w bazie");

                return CreatedAtAction(nameof(GetClient), new { clientId = added.Id }, added);
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }
    }
}