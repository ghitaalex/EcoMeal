using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoMeal.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessTypeController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        public BusinessTypeController(EcoMealDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessTypeDTO>>> GetAllTypes()
        {
            var types = await _context.BusinessType
                .Select(t => new BusinessTypeDTO
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();

            return Ok(types);
        }
    }
}
