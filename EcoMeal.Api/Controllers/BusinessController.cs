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
        private readonly string _containerName = "ecomeal-businesses";
        public BusinessController(EcoMealDbContext context, BlobStorageService blobStorageService) {
            _context = context;
            _blobStorageService = blobStorageService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessDTO>>> GetBusinesses()
        {
            var businessesDTOs = await _context.Business.Include(b => b.BusinessType)
                .Select(b => new BusinessDTO{
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                Description = b.Description,
                Contact = b.Contact,
                BusinessTypeName = b.BusinessType.Name,
                BusinessImageUrl = b.BusinessImageUrl
                }).ToListAsync();

            return Ok(businessesDTOs);
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
        public async Task<ActionResult<BusinessDetailsDTO>> GetOneById(int id)
        {
            var business = await _context.Business
                .Select(b => new BusinessDetailsDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    Description = b.Description,
                    Contact = b.Contact,
                    BusinessTypeName = b.BusinessType.Name,
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

            _context.Business.Add(new Business
            {
                Name = business.Name,
                Address = business.Address,
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

            existingBusiness.Name = business.Name;
            existingBusiness.Address = business.Address;
            existingBusiness.Description = business.Description;
            existingBusiness.Contact = business.Contact;
            existingBusiness.BusinessTypeId = business.BusinessTypeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
