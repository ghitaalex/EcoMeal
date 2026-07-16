using EcoMeal.Api.Constants;
using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using EcoMeal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BusinessController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly GeocodingService _geocodingService;
        private readonly DrivingDistanceService _drivingDistanceService;
        private readonly string _containerName = "ecomeal-businesses";
        public BusinessController(EcoMealDbContext context, BlobStorageService blobStorageService, GeocodingService geocodingService, DrivingDistanceService drivingDistanceService) {
            _context = context;
            _blobStorageService = blobStorageService;
            _geocodingService = geocodingService;
            _drivingDistanceService = drivingDistanceService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessDTO>>> GetBusinesses()
        {
            var businessesDTOs = await _context.Business.Include(b => b.BusinessType)
                .Select(b => new BusinessDTO{
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                Description = b.Description,
                Contact = b.Contact,
                BusinessTypeName = b.BusinessType.Name,
                BusinessImageUrl = b.BusinessImageUrl,
                AverageRating = b.Packages
                    .SelectMany(p => p.Orders)
                    .SelectMany(o => _context.Review.Where(r => r.OrderId == o.Id))
                    .Select(r => (double?)r.Rating)
                    .Average() ?? 0,
                ReviewCount = b.Packages
                    .SelectMany(p => p.Orders)
                    .SelectMany(o => _context.Review.Where(r => r.OrderId == o.Id))
                    .Count()
                }).ToListAsync();

            return Ok(businessesDTOs);
        }

        [HttpPost("driving-distances")]
        public async Task<ActionResult<IEnumerable<DrivingDistanceDTO>>> GetDrivingDistances(UserLocationDTO userLocation)
        {
            if (userLocation.Latitude < -90 || userLocation.Latitude > 90 ||
                userLocation.Longitude < -180 || userLocation.Longitude > 180)
            {
                return BadRequest("Invalid location");
            }

            var businesses = await _context.Business
                .Where(b => b.Latitude.HasValue && b.Longitude.HasValue)
                .Select(b => new BusinessLocation
                {
                    Id = b.Id,
                    Latitude = b.Latitude!.Value,
                    Longitude = b.Longitude!.Value
                })
                .ToListAsync();

            var distances = await _drivingDistanceService.GetDrivingDistancesAsync(
                userLocation.Latitude,
                userLocation.Longitude,
                businesses);

            return Ok(distances);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> DeleteBusinsess(int id) { 
            /*int count = await _context.Business.Where(b => b.Id == id).ExecuteDeleteAsync();
            if (count == 0)
            {
                return NotFound("Couldn't find the business");
            }*/
            var business = await _context.Business.FirstOrDefaultAsync(b => b.Id == id);
            if (business == null) {
                return NotFound("Could not find business");
            }
            if (!(business.BusinessImageUrl == null))
            {
                await _blobStorageService.DeleteBlobAsync(_containerName, business.BusinessImageUrl);
            }
            _context.Business.Remove(business);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessDTO>> GetOneById(int id)
        {
            var business = await _context.Business
                .Include (b => b.Packages)
                .ThenInclude(p => p.PackageType)
                .Select(b => new BusinessDetailsDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude,
                    Description = b.Description,
                    Contact = b.Contact,
                    BusinessTypeName = b.BusinessType.Name,
                    Packages = b.Packages
                    .Select(p => new PackageDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        NoPackages = p.NoPackages,
                        AvailablePackages = p.NoPackages,
                        PickUpStart = p.PickUpStart,
                        PickUpEnd = p.PickUpEnd,
                        PackageTypeName = p.PackageType.Name,
                        PackageImageUrl = p.PackageImageUrl,
                    }),
                    BusinessImageUrl = b.BusinessImageUrl
                })
                .FirstOrDefaultAsync(b => b.Id == id);
            if (business is null)
            {
                return NotFound();
            }

            return Ok(business);
        }

        [HttpPost]
        public async Task<IActionResult> AddBusiness([FromForm] BusinessAddDTO business)
        {
            string? imageUrl = null;
            if (business.BusinessImage != null)
            {
                imageUrl = await _blobStorageService.UploadImageAsync(_containerName, business.BusinessImage);
            }

            var coordinates = await _geocodingService.GeocodeAsync(business.Address);

            _context.Business.Add(new Business
            {
                Name = business.Name,
                Address = business.Address,
                Latitude = coordinates?.Latitude,
                Longitude = coordinates?.Longitude,
                Description = business.Description,
                Contact = business.Contact,
                BusinessTypeId = business.BusinessTypeId,
                BusinessType = null!,
                BusinessImageUrl = imageUrl
            });

            await _context.SaveChangesAsync();
            return Created();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditBusiness(int id, [FromForm] BusinessAddDTO business)
        {
            var existingBusiness = await _context.Business.FirstOrDefaultAsync(b => b.Id == id);
            if (existingBusiness == null)
            {
                return NotFound("Couldn't find the business");
            }

            if (business.BusinessImage != null)
            {
                if (!string.IsNullOrEmpty(existingBusiness.BusinessImageUrl))
                {
                    await _blobStorageService.DeleteBlobAsync(_containerName, existingBusiness.BusinessImageUrl);
                }
                existingBusiness.BusinessImageUrl = await _blobStorageService.UploadImageAsync(_containerName, business.BusinessImage);
            }

            var addressChanged = !string.Equals(existingBusiness.Address.Trim(), business.Address.Trim(), StringComparison.OrdinalIgnoreCase);

            existingBusiness.Name = business.Name;
            existingBusiness.Address = business.Address;
            existingBusiness.Description = business.Description;
            existingBusiness.Contact = business.Contact;
            existingBusiness.BusinessTypeId = business.BusinessTypeId;

            if (addressChanged || existingBusiness.Latitude is null || existingBusiness.Longitude is null)
            {
                var coordinates = await _geocodingService.GeocodeAsync(business.Address);
                existingBusiness.Latitude = coordinates?.Latitude;
                existingBusiness.Longitude = coordinates?.Longitude;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
