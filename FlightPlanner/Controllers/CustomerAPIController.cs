using FlightPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PageResult = FlightPlanner.Models.PageResult;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors]
    public class CustomerApiController : ControllerBase
    {
        [HttpGet]
        [Route("airports")]
        public IActionResult SearchAirports(string search)
        {
            var airports = FlightStorage.FindAirports(search);
            return Ok(airports);
        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult SearchFlights(int id)
        {
            var flight = FlightStorage.GetFlight(id);
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
    }
}
