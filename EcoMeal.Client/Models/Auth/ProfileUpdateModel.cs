using System.ComponentModel.DataAnnotations;

namespace EcoMeal.Client.Models.Auth;

public class ProfileUpdateModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact is required")]
    [Phone(ErrorMessage = "Invalid contact number")]
    public string Contact { get; set; } = string.Empty;
}
