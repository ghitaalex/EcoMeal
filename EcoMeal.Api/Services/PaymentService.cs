using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace EcoMeal.Api.Services
{
    public class PaymentService
    {
        private readonly EcoMealDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<PaymentService> _logger;
        private readonly StripeClient? _stripeClient;
        private readonly string? _webhookSecret;
        private readonly string _clientUrl;

        public PaymentService(
            EcoMealDbContext context,
            EmailService emailService,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _webhookSecret = configuration["Stripe:WebhookSecret"];
            _clientUrl = (configuration["Stripe:ClientBaseUrl"] ?? "https://localhost:5000")
                .TrimEnd('/');

            var secretKey = configuration["Stripe:SecretKey"];
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                _stripeClient = new StripeClient(secretKey);
            }
        }

        public bool IsConfigured => _stripeClient is not null &&
                                    !string.IsNullOrWhiteSpace(_webhookSecret);

        public async Task<Session> CreateCheckoutSessionAsync(
            Order order,
            Package package,
            User user)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                Mode = "payment",
                CustomerEmail = user.Email,
                ClientReferenceId = order.Id.ToString(),
                SuccessUrl = $"{_clientUrl}/payment/success",
                CancelUrl = $"{_clientUrl}/business/{package.BusinessId}",
                ExpiresAt = DateTime.UtcNow.AddMinutes(31),
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "ron",
                            UnitAmount = decimal.ToInt64(order.TotalAmount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = package.Name
                            }
                        }
                    }
                ]
            };

            return await new SessionService(GetStripeClient()).CreateAsync(options);
        }

        public async Task CompletePaymentAsync(Session session)
        {
            if (session.PaymentStatus != "paid" ||
                !int.TryParse(session.ClientReferenceId, out var orderId))
            {
                return;
            }

            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Package)
                    .ThenInclude(p => p.Business)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null ||
                order.Status != "Placed" ||
                order.PaymentStatus != "Pending" ||
                !order.StockReserved)
            {
                return;
            }

            if (session.AmountTotal != decimal.ToInt64(order.TotalAmount * 100))
            {
                return;
            }

            order.PaymentStatus = "Paid";
            order.Status = "Pending";
            await _context.SaveChangesAsync();

            await SendConfirmationEmailAsync(order, order.User, order.Package);
        }

        public async Task ExpirePaymentAsync(Session session)
        {
            if (!int.TryParse(session.ClientReferenceId, out var orderId))
            {
                return;
            }

            var order = await _context.Order
                .Include(o => o.Package)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null ||
                order.Status != "Placed" ||
                order.PaymentStatus != "Pending" ||
                !order.StockReserved)
            {
                return;
            }

            order.Package.NoPackages += 1;
            order.StockReserved = false;
            order.PaymentStatus = "Expired";
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }

        public async Task SendConfirmationEmailAsync(Order order, User user, Package package)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            try
            {
                var userName = user.Name ?? user.Email;
                var body = await _emailService.LoadTemplateAsync("OrderConfirmed", new Dictionary<string, string>
                {
                    { "UserName", userName },
                    { "PackageName", package.Name },
                    { "BusinessName", package.Business.Name },
                    { "Price", order.TotalAmount.ToString("F2") },
                    { "PickUpStart", package.PickUpStart.ToString("HH:mm") },
                    { "PickUpEnd", package.PickUpEnd.ToString("HH:mm") }
                });

                await _emailService.SendEmailAsync(
                    user.Email,
                    userName,
                    "Your EcoMeal Order is Confirmed!",
                    body);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not send email for order {OrderId}", order.Id);
            }
        }

        public Event GetWebhookEvent(string json, string signature)
        {
            if (string.IsNullOrWhiteSpace(_webhookSecret))
            {
                throw new InvalidOperationException("Stripe webhook secret is missing");
            }

            return EventUtility.ConstructEvent(
                json,
                signature,
                _webhookSecret,
                tolerance: 300,
                throwOnApiVersionMismatch: false);
        }

        private StripeClient GetStripeClient()
        {
            return _stripeClient ?? throw new InvalidOperationException("Stripe is not configured");
        }
    }
}
