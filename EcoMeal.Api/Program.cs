using Azure.Identity;
using Azure.Storage.Blobs;
using EcoMeal.Api.Constants;
using EcoMeal.Api.Entities;
using EcoMeal.Api.Infrastructure;
using EcoMeal.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
/*builder.Configuration.AddAzureKeyVault(
    new Uri("https://ecomeal-vault.vault.azure.net/"),
    new DefaultAzureCredential());
*/
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EcoMealDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<EcoMealDbContext>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorSite", policy =>
    {
        policy.WithOrigins("http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton(x =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage")));
builder.Services.AddScoped<BlobStorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "EcoMeal API");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorSite");
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var roles = new[] { UserRoles.Admin, UserRoles.User };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }
    }
}

app.Run();
