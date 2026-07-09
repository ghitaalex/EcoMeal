using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using EcoMeal.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Controllers
{
    [ApiController]
    [Route("api/business/{id}/package")]
    public class PackageController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly string _containerName = "ecomeal-packages";
        public PackageController(EcoMealDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> AddPackageToBusiness(int id, [FromForm] PackageAddDTO package)
        {
            string? imageUrl = null;
            if (package.PackageImage != null)
            {
                imageUrl = await _blobStorageService.UploadImageAsync(_containerName, package.PackageImage);
            }
            _context.Package.Add(new Package
            {
                Name = package.Name,
                Description = package.Description,
                Price = package.Price,
                PickUpStart = package.StartPickup,
                PickUpEnd = package.EndPickup,
                PackageTypeId = package.PackageTypeId,
                BusinessId = id,
                PackageImageUrl = imageUrl
            });

            await _context.SaveChangesAsync();
            return Created();
        }

        [HttpDelete("{PackageId}")]
        public async Task<ActionResult> DeletePackage(int PackageId)
        {
            /*int count = await _context.Package.Where(p => p.Id == PackageId).ExecuteDeleteAsync();
            if (count == 0)
            {
                return NotFound("Couldn't find the package");
            }*/
            var package = await _context.Package.FirstOrDefaultAsync(b => b.Id == PackageId);
            if (package == null)
            {
                return NotFound("Could not find package");
            }
            if (!(package.PackageImageUrl == null))
            {
                await _blobStorageService.DeleteBlobAsync(_containerName, package.PackageImageUrl);
            }
            _context.Package.Remove(package);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{PackageId}")]
        public async Task<IActionResult> EditPackage(int PackageId, [FromForm] PackageAddDTO package)
        {
            var existingPackage = await _context.Package.FirstOrDefaultAsync(p => p.Id == PackageId);
            if (existingPackage == null)
            {
                return NotFound("Couldn't find the package");
            }
            if (package.PackageImage != null)
            {
                if (!string.IsNullOrEmpty(existingPackage.PackageImageUrl))
                {
                    await _blobStorageService.DeleteBlobAsync(_containerName, existingPackage.PackageImageUrl);
                }
                existingPackage.PackageImageUrl = await _blobStorageService.UploadImageAsync(_containerName, package.PackageImage);
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
                    PackageTypeName = p.PackageType.Name,
                    PackageImageUrl = p.PackageImageUrl
                })
                .ToListAsync();

            return Ok(packages);
        }
    }
}
