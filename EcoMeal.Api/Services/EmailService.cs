using System.Text;
using System.Net.Http.Headers;

namespace EcoMeal.Api.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _senderEmail;
        private readonly bool _isConfigured;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _httpClient = new HttpClient();
            _env = env;

            var apiKey = configuration["MailjetApiKey"];
            var secretKey = configuration["MailjetSecretKey"];
            _senderEmail = configuration["MailjetSenderEmail"];

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(_senderEmail))
                return;

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{apiKey}:{secretKey}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
            _isConfigured = true;
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
            if (!_isConfigured)
                return;

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

            using var response = await _httpClient.PostAsJsonAsync("https://api.mailjet.com/v3.1/send", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
