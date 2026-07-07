using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        public BusinessController(EcoMealDbContext context) {
            _context = context;
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
                BusinessTypeName = b.BusinessType.Name
                }).ToListAsync();

            return Ok(businessesDTOs);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBusinsess(int id) { 
            int count = await _context.Business.Where(b => b.Id == id).ExecuteDeleteAsync();
            if (count == 0)
            {
                return NotFound("Couldn't find the business");
            }
            return NoContent();
        }
    }
}
