using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp9.Data;
using WebApp9.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly MasterContext _context;

        public TripsController(MasterContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var tripsQuery = _context.Trips
                .Include(t => t.CountryTrips)
                .ThenInclude(ct => ct.Country)
                .Include(t => t.ClientTrips)
                .ThenInclude(ct => ct.Client)
                .OrderByDescending(t => t.DateFrom);

            var totalTrips = await tripsQuery.CountAsync();
            var trips = await tripsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                pageNum = page,
                pageSize = pageSize,
                allPages = (int)Math.Ceiling((double)totalTrips / pageSize),
                trips = trips.Select(t => new
                {
                    t.Name,
                    t.Description,
                    DateFrom = t.DateFrom.ToString("yyyy-MM-dd"),
                    DateTo = t.DateTo.ToString("yyyy-MM-dd"),
                    t.MaxPeople,
                    Countries = t.CountryTrips.Select(ct => new CountryDTO { IdCountry = ct.Country.IdCountry, Name = ct.Country.Name }),
                    Clients = t.ClientTrips.Select(ct => new ClientDTO { IdClient = ct.Client.IdClient, FirstName = ct.Client.FirstName, LastName = ct.Client.LastName })
                })
            };

            return Ok(response);
        }
    }
}