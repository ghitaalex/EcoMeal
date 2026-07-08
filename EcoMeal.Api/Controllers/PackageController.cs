using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Controllers
{
    [ApiController]
    [Route("api/business/{id}/package")]
    public class PackageController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        public PackageController(EcoMealDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPackageToBusiness(int id, [FromBody] PackageAddDTO package)
        {
            _context.Package.Add(new Package
            {
                Name = package.Name,
                Description = package.Description,
                Price = package.Price,
                PickUpStart = package.StartPickup,
                PickUpEnd = package.EndPickup,
                PackageTypeId = package.PackageTypeId,
                BusinessId = id
            });

            await _context.SaveChangesAsync();
            return Created();
        }

        [HttpDelete("{PackageId}")]
        public async Task<ActionResult> DeletePackage(int PackageId)
        {
            int count = await _context.Package.Where(p => p.Id == PackageId).ExecuteDeleteAsync();
            if (count == 0)
            {
                return NotFound("Couldn't find the package");
            }
            return NoContent();
        }

        [HttpPut("{PackageId}")]
        public async Task<IActionResult> EditPackage(int PackageId, [FromBody] PackageAddDTO package)
        {
            var existingPackage = await _context.Package.FirstOrDefaultAsync(p => p.Id == PackageId);
            if (existingPackage == null)
            {
                return NotFound("Couldn't find the package");
            }

            existingPackage.Name = package.Name;
            existingPackage.Description = package.Description;
            existingPackage.Price = package.Price;
            existingPackage.PickUpStart = package.StartPickup;
            existingPackage.PickUpEnd = package.EndPickup;
            existingPackage.PackageTypeId = package.PackageTypeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<List<PackageDTO>>> GetAllPackages(int id)
        {
            var packages = await _context.Package
                .Where(p => p.BusinessId == id)
                .Select(p => new PackageDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PickUpStart = p.PickUpStart,
                    PickUpEnd = p.PickUpEnd,
                    PackageTypeName = p.PackageType.Name
                })
                .ToListAsync();

            return Ok(packages);
        }
    }
}
