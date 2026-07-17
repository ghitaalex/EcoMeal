using EcoMeal.Api.Constants;
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
        private readonly PaymentService _paymentService;

        public OrderController(EcoMealDbContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<CheckoutSessionDTO>> CreateOrder([FromBody] OrderCreateDTO request)
        {
            if (request.PayWithCard && !_paymentService.IsConfigured)
            {
                return StatusCode(503, "Stripe is not configured");
            }

            var userId = GetCurrentUserID();
            var user = await _context.Users.FindAsync(userId);
            var package = await _context.Package
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId);

            if (user is null)
            {
                return Unauthorized();
            }
            if (package is null)
            {
                return NotFound("Package not found");
            }
            if (package.NoPackages <= 0)
            {
                return BadRequest("Package not available anymore");
            }
            if (request.PayWithCard && package.Price < 2)
            {
                return BadRequest("The minimum Stripe payment is 2.00 RON");
            }

            package.NoPackages -= 1;
            var order = new Order
            {
                UserId = userId,
                PackageId = package.Id,
                Status = request.PayWithCard ? "Placed" : "Pending",
                Date = DateTime.UtcNow,
                TotalAmount = package.Price,
                PaymentStatus = request.PayWithCard ? "Pending" : "Not required",
                StockReserved = true
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            if (!request.PayWithCard)
            {
                await _paymentService.SendConfirmationEmailAsync(order, user, package);
                return Ok(new CheckoutSessionDTO
                {
                    OrderId = order.Id,
                    Status = order.Status
                });
            }

            try
            {
                var session = await _paymentService.CreateCheckoutSessionAsync(order, package, user);
                return Ok(new CheckoutSessionDTO
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    CheckoutUrl = session.Url
                });
            }
            catch
            {
                package.NoPackages += 1;
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
                return StatusCode(502, "Could not start Stripe Checkout");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderGetDTO>>> GetOrders()
        {
            var userId = GetCurrentUserID();
            var orders = await _context.Order
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Date)
                .Select(o => new OrderGetDTO
                {
                    Id = o.Id,
                    Date = o.Date,
                    Status = o.Status,
                    Price = o.TotalAmount,
                    BusinessId = o.Package.BusinessId,
                    BusinessName = o.Package.Business.Name,
                    PackageName = o.Package.Name,
                    IsReviewed = _context.Review.Any(r => r.OrderId == o.Id),
                    ReviewRating = _context.Review.Where(r => r.OrderId == o.Id)
                        .Select(r => (int?)r.Rating).FirstOrDefault(),
                    ReviewComment = _context.Review.Where(r => r.OrderId == o.Id)
                        .Select(r => r.Comment).FirstOrDefault()
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpGet("business/{businessId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<List<OrderGetDTO>>> GetOrdersByBusiness(int businessId)
        {
            var orders = await _context.Order
                .Where(o => o.Package.BusinessId == businessId &&
                    (o.PaymentStatus == "Paid" || o.PaymentStatus == "Not required"))
                .OrderByDescending(o => o.Date)
                .Select(o => new OrderGetDTO
                {
                    Id = o.Id,
                    Date = o.Date,
                    Status = o.Status,
                    Price = o.TotalAmount,
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
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            if (status != "Pending" && status != "Completed")
            {
                return BadRequest("Only Pending and Completed are supported");
            }

            var order = await _context.Order.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
            {
                return NotFound();
            }
            if (order.PaymentStatus != "Paid" && order.PaymentStatus != "Not required")
            {
                return BadRequest("The card payment has not been completed");
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int GetCurrentUserID()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdValue!);
        }
    }
}
