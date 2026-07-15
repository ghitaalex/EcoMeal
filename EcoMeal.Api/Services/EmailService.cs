using System.Text;
using System.Text.Json;

namespace EcoMeal.Api.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<EmailService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["MailtrapApiKey"]
                ?? throw new InvalidOperationException("MailtrapApiKey is not configured.");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
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
                from = new { email = "hello@demomailtrap.co", name = "EcoMeal" },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                html = body
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("https://send.api.mailtrap.io/api/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Mailtrap rejected an email to {Recipient}. Status: {StatusCode}. Response: {ResponseBody}",
                    toEmail,
                    (int)response.StatusCode,
                    responseBody);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
