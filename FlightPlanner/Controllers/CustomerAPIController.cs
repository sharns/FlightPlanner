using System.Collections.Generic;
using System.Linq;
using FlightPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PageResult = FlightPlanner.Models.PageResult;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors]
    public class CustomerApiController : ControllerBase
    {
        private readonly FlightPlannerDbContext _context;
        private static readonly object _lock = new object();

        public CustomerApiController(FlightPlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("airports")]
        public IActionResult SearchAirports(string search)
        {
            lock (_lock)
            {
                var airports = SearchAirport(search);
                return Ok(airports);
            }
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult SearchFlights(int id)
        {
            var flight = _context.Flights
                .Include(f => f.From)
                .Include(f => f.To)
                .SingleOrDefault(f => f.Id == id);

            if (flight == null)
                return NotFound();

            return Ok(flight);
        }

        [HttpPost]
        [Route("flights/search")]
        public IActionResult InvalidRequest(SearchFlightRequest request)
        {
            if (!FlightStorage.IsValidFlight(request))
                return BadRequest(request);
            return Ok(FlightStorage.ValidFlight(request));
        }

        private List<Airport> SearchAirport(string search)
        {
            lock (_lock)
            {
                search = search.ToLower().Trim();
                var fromAirports = _context.Flights.Where(x =>
                        x.From.AirportName.ToLower().Trim().Contains(search) ||
                        x.From.City.ToLower().Trim().Contains(search) ||
                        x.From.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.From)
                    .ToList();

                var toAirports = _context.Flights.Where(x =>
                        x.To.AirportName.ToLower().Trim().Contains(search) ||
                        x.To.City.ToLower().Trim().Contains(search) ||
                        x.To.Country.ToLower().Trim().Contains(search))
                    .Select(x => x.To)
                    .ToList();

                return fromAirports.Concat(toAirports).ToList();
            }
        }
    }
}
