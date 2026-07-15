using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace EcoMeal.Api.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _senderEmail;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<EmailService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["MailjetApiKey"]
                ?? throw new InvalidOperationException("MailjetApiKey is not configured.");
            _secretKey = configuration["MailjetSecretKey"]
                ?? throw new InvalidOperationException("MailjetSecretKey is not configured.");
            _senderEmail = configuration["MailjetSenderEmail"]
                ?? throw new InvalidOperationException("MailjetSenderEmail is not configured.");

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_apiKey}:{_secretKey}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
            _env = env;
            _logger = logger;
        }

        public async Task<string> LoadTemplateAsync(string templateName, Dictionary<string, string> placeholders)
        {
            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", $"{templateName}.html");
            var template = await File.ReadAllTextAsync(path);

            foreach (var (key, value) in placeholders)
            {
                template = template.Replace($"{{{{{key}}}}}", value);
            }

            return template;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            var payload = new
            {
                Messages = new[]
                {
                    new
                    {
                        From = new { Email = _senderEmail, Name = "EcoMeal" },
                        To = new[] { new { Email = toEmail, Name = toName } },
                        Subject = subject,
                        HTMLPart = body
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("https://api.mailjet.com/v3.1/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Mailjet rejected an email to {Recipient}. Status: {StatusCode}. Response: {ResponseBody}",
                    toEmail,
                    (int)response.StatusCode,
                    responseBody);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
