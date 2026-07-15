using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Models;
using EcoMeal.Api.Services;
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
        private readonly EmailService _emailService;

        public OrderController(EcoMealDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [HttpPost]
        public async Task<ActionResult<OrderGetDTO>> CreateOrder([FromBody] OrderCreateDTO request) {
            var userId = GetCurrentUserID();
            var user = await _context.Users.FindAsync(userId);
            var package = await _context.Package.Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId);

            if (package is null)
            {
                return NotFound("Package not found");
            }

            if (package.NoPackages <= 0)
            {
                return BadRequest("Package not available anymore");
            }

            package.NoPackages -= 1;

            var order = new Order
            {
                UserId = userId,
                PackageId = request.PackageId,
                Status = "Pending",
                Date = DateTime.UtcNow
            };
            _context.Order.Add(order);
            await _context.SaveChangesAsync();
            var body = await _emailService.LoadTemplateAsync("OrderConfirmed", new Dictionary<string, string>
                {
                    { "UserName", user.Name },
                    { "PackageName", package.Name },
                    { "BusinessName", package.Business.Name },
                    { "Price", package.Price.ToString("F2") },
                    { "PickUpStart", package.PickUpStart.ToString("HH:mm") },
                    { "PickUpEnd", package.PickUpEnd.ToString("HH:mm") }
                });

            await _emailService.SendEmailAsync(user.Email, user.Name, "Your EcoMeal Order is Confirmed! 🎉", body);

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
                    PackageName = o.Package.Name,
                    IsReviewed = _context.Review.Any(r => r.OrderId == o.Id),
                    ReviewRating = _context.Review.Where(r => r.OrderId == o.Id).Select(r => (int?)r.Rating).FirstOrDefault(),
                    ReviewComment = _context.Review.Where(r => r.OrderId == o.Id).Select(r => r.Comment).FirstOrDefault()
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
                    UserContact = o.User.Contact,
                    IsReviewed = _context.Review.Any(r => r.OrderId == o.Id)
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Order
                .Include(o => o.Package)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            var previousStatus = order.Status;
            var isPreviouslyCancelled = IsCancelledStatus(previousStatus);
            var isNowCancelled = IsCancelledStatus(status);

            if (!isPreviouslyCancelled && isNowCancelled)
            {
                order.Package.NoPackages += 1;
            }
            else if (isPreviouslyCancelled && !isNowCancelled)
            {
                if (order.Package.NoPackages <= 0)
                {
                    return BadRequest("Package not available anymore");
                }

                order.Package.NoPackages -= 1;
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static bool IsCancelledStatus(string? status)
        {
            return string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase);
        }

        private int GetCurrentUserID()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdValue!);
        }
    }
}
