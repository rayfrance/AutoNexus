using AutoNexus.Application.Common;
using AutoNexus.Domain.Entities;
using AutoNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNexus.Web.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            int pageSize = 5;

            IQueryable<Vehicle> vehiclesQuery = GetVehicleQuery(searchString);

            PaginatedList<Vehicle> paginatedResult = await PaginatedList<Vehicle>.CreateAsync(vehiclesQuery, pageNumber ?? 1, pageSize);

            return View(paginatedResult);
        }

        private IQueryable<Vehicle> GetVehicleQuery(string searchString)
        {
            IQueryable<Vehicle> vehiclesQuery = GetBaseQuery();

            vehiclesQuery = ApplySearchFilter(vehiclesQuery, searchString);

            vehiclesQuery = ApplySorting(vehiclesQuery);
            return vehiclesQuery;
        }

        private IQueryable<Vehicle> GetBaseQuery()
        {
            return _context.Vehicles
                .Include(v => v.Manufacturer)
                .AsNoTracking();
        }

        private IQueryable<Vehicle> ApplySearchFilter(IQueryable<Vehicle> query, string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return query;

            return query.Where(s => s.Model.Contains(searchString)
                                 || s.Manufacturer.Name.Contains(searchString));
        }

        private IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query)
        {
            return query.OrderByDescending(v => v.CreatedAt);
        }
    }
}