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
    public class ReviewController : ControllerBase
    {
        private readonly EcoMealDbContext _context;
        public ReviewController(EcoMealDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateReview([FromBody] ReviewRequest request)
        {
            var userId = GetCurrentUserID();

            var order = await _context.Order
                .Include(o => o.Package)
                .Where(o => o.Id == request.OrderId
                            && o.UserId == userId
                            && o.Package.BusinessId == request.BusinessId)
                .FirstOrDefaultAsync();

            if (order is null)
            {
                return BadRequest("No matching order found for this business.");
            }

            if (!order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You can only review a completed order.");
            }

            var existingReview = await _context.Review
                .AnyAsync(r => r.OrderId == order.Id);

            if (existingReview)
            {
                return BadRequest("You have already reviewed this order.");
            }

            var review = new Review
            {
                UserId = userId,
                OrderId = order.Id,
                Rating = request.Rating,
                Comment = request.Review,
                CreatedAt = DateTime.UtcNow
            };

            _context.Review.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { review.Id, review.Rating, review.Comment, review.CreatedAt });
        }

        [HttpGet("business/{businessId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetReviewsByBusiness(int businessId)
        {
            var reviews = await _context.Review
                .Include(r => r.User)
                .Include(r => r.Order)
                    .ThenInclude(o => o.Package)
                .Where(r => r.Order.Package.BusinessId == businessId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserName = r.User.Name
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserID();

            var review = await _context.Review
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review is null)
            {
                return NotFound();
            }

            _context.Review.Remove(review);
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
