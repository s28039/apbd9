using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.DTOs;
using YourNamespace.Models;
using System.Linq;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MasterContext _context;

        public ClientsController(MasterContext context)
        {
            _context = context;
        }

        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var client = await _context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
            {
                return NotFound(new { message = "Client not found" });
            }

            if (client.ClientTrips.Any())
            {
                return BadRequest(new { message = "Client has assigned trips" });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientDTO clientDto)
        {
            if (await _context.Clients.AnyAsync(c => c.Pesel == clientDto.Pesel))
            {
                return BadRequest(new { message = "Client with this PESEL already exists" });
            }

            var trip = await _context.Trips
                .Include(t => t.ClientTrips)
                .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

            if (trip == null || trip.DateFrom <= DateTime.Now)
            {
                return BadRequest(new { message = "Trip does not exist or has already started" });
            }

            if (trip.ClientTrips.Any(ct => ct.Client.Pesel == clientDto.Pesel))
            {
                return BadRequest(new { message = "Client is already assigned to this trip" });
            }

            var client = new Client
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                Email = clientDto.Email,
                Telephone = clientDto.Telephone,
                Pesel = clientDto.Pesel
            };

            var clientTrip = new ClientTrip
            {
                Client = client,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = clientDto.PaymentDate
            };

            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AssignClientToTrip), new { idTrip = idTrip }, clientTrip);
        }
    }
}
