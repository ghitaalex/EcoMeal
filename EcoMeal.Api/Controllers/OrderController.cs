using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoMeal.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        public OrderController(EcoMealDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<ActionResult<OrderGetDTO>> CreateOrder([FromBody] OrderCreateDTO request) {
            var userId = GetCurrentUserID();

            var package = await _context.Package.Include(p => p.Business)
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId);

            if (package is null)
            {
                return NotFound("Package not found");
            }

            if (package.Orders.Any()) {
                return BadRequest("Package not available anymore"); 
            }

            var order = new Order
            {
                UserId = userId,
                PackageId = request.PackageId,
                Status = "Pending",
                Date = DateTime.UtcNow
            };
            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new OrderGetDTO
            {
                Id = order.Id,
                PackageName = package.Name,
                Status = order.Status,
                Price = package.Price,
                BusinessId = package.BusinessId,
                BusinessName = package.Business.Name,
                Date = order.Date,
                UserName = order.User?.Name,
                UserContact = order.User?.Contact
            });
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderGetDTO>>> GetOrders()
        {
            var userId = GetCurrentUserID();
            var orders = await _context.Order
                .Where(o =>  o.UserId == userId)
                .OrderByDescending(o => o.Date)
                .Select(o => new OrderGetDTO
                {
                    Id = o.Id,
                    Date = o.Date,
                    Status = o.Status,
                    Price = o.Package.Price,
                    BusinessId = o.Package.BusinessId,
                    BusinessName = o.Package.Business.Name,
                    PackageName = o.Package.Name
                }).ToListAsync();
                
            return Ok(orders);
        }

        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<List<OrderGetDTO>>> GetOrdersByBusiness(int businessId)
        {
            var orders = await _context.Order
                .Include(o => o.User)
                .Where(o => o.Package.BusinessId == businessId)
                .OrderByDescending(o => o.Date)
                .Select(o => new OrderGetDTO
                {
                    Id = o.Id,
                    Date = o.Date,
                    Status = o.Status,
                    Price = o.Package.Price,
                    BusinessId = o.Package.BusinessId,
                    BusinessName = o.Package.Business.Name,
                    PackageName = o.Package.Name,
                    UserName = o.User.Name,
                    UserContact = o.User.Contact
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Order.FindAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        private int GetCurrentUserID()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdValue!);
        }
    }
}
