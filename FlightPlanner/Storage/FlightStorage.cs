﻿using System;
using FlightPlanner.Models;
using System.Collections.Generic;
using System.Linq;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static List<Flight> _flights = new List<Flight>();
        private static int _id;
        private static readonly object _lock = new object();

        public static Flight AddFlight(AddFlightRequest request)
        {
            lock (_lock)
            {
                var flight = new Flight
                {
                    From = request.From,
                    To = request.To,
                    ArrivalTime = request.ArrivalTime,
                    DepartureTime = request.DepartureTime,
                    Carrier = request.Carrier,
                    Id = ++_id
                };
                _flights.Add(flight);

                return flight;
            }
        }

        public static Flight GetFlight(int id)
        {
            lock (_lock)
            {
                return _flights.SingleOrDefault(f => f.Id == id);
            }
        }

        public static void DeleteFlight(int id)
        {
            lock (_lock)
            {
                var flight = GetFlight(id);
                if (flight != null)
                {
                    _flights.Remove(flight);
                }
            }
        }

        public static List<Airport> FindAirports(string input)
        {
            lock (_lock)
            {
                input = input.ToLower().Trim();
                var fromAirports = _flights.Where(f => f.From.AirportName.ToLower().Trim().Contains(input)
                                                       || f.From.City.ToLower().Trim().Contains(input)
                                                       || f.From.Country.ToLower().Trim().Contains(input)).
                                                        Select(a => a.From).ToList();
                var toAirports = _flights.Where(f => f.To.AirportName.ToLower().Trim().Contains(input)
                                                     || f.To.City.ToLower().Trim().Contains(input)
                                                     || f.To.Country.ToLower().Trim().Contains(input)).
                                                        Select(f => f.To).ToList();

                return fromAirports.Concat(toAirports).ToList();
            }
        }

        public static void ClearFlights()
        {
            lock (_lock)
            {
                _flights.Clear();
                _id = 0;
            }
        }

        public static PageResult ValidFlight(SearchFlightRequest request)
        {
            lock (_lock)
            {
                return new PageResult(_flights);
            }
        }

        public static bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _flights.Any(f => f.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                                  f.DepartureTime == request.DepartureTime &&
                                  f.ArrivalTime == request.ArrivalTime &&
                                  f.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim() &&
                                  f.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim());
            }
        }

        public static bool IsValidFlight(SearchFlightRequest request)
        {
            lock (_lock)
            {
                if (request.From == request.To)
                    return false;

                if (request.From == null || request.To == null || request.DepartureDate == null)
                    return false;

                return true;
            }
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
    }
}
