using System;
using FlightPlanner.Models;
using System.Collections.Generic;
using System.Linq;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static readonly object _lock = new object();

        public static Flight ConvertToFlight(AddFlightRequest request)
        {
            var flight = new Flight
            {
                From = request.From,
                To = request.To,
                ArrivalTime = request.ArrivalTime,
                DepartureTime = request.DepartureTime,
                Carrier = request.Carrier,
            };

            return flight;
        }

        public static bool IsValid(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (request == null)
                    return false;

                if (string.IsNullOrEmpty(request.ArrivalTime) || string.IsNullOrEmpty(request.Carrier) ||
                    string.IsNullOrEmpty(request.DepartureTime))
                    return false;

                if (request.From == null || request.To == null)
                    return false;

                if (string.IsNullOrEmpty(request.From.AirportName) || string.IsNullOrEmpty(request.From.City) ||
                    string.IsNullOrEmpty(request.From.Country))
                    return false;

                if (string.IsNullOrEmpty(request.To.AirportName) || string.IsNullOrEmpty(request.To.City) ||
                    string.IsNullOrEmpty(request.To.Country))
                    return false;

                if (request.From.Country.ToLower().Trim() == request.To.Country.ToLower().Trim() &&
                    request.From.City.ToLower().Trim() == request.To.City.ToLower().Trim()
                    && request.From.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim())
                    return false;

                var arrivalTime = DateTime.Parse(request.ArrivalTime);
                var departureTime = DateTime.Parse(request.DepartureTime);

                if (arrivalTime <= departureTime)
                {
                    return false;
                }

                return true;
            }
        }

        public static PageResult SearchByParams(string from, string to, string date, FlightPlannerDbContext context)
        {
            lock (_lock)
            {
                var filteredFlight = context.Flights.Where(f => f.From.AirportName == from || f.To.AirportName == to || f.DepartureTime == date).ToList();

                return new PageResult(filteredFlight);
            }
        }
    }
}
