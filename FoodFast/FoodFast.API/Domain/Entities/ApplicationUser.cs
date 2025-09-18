using Microsoft.AspNetCore.Identity;

namespace FoodFast.API.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public bool IsDriver { get; set; }
    public bool IsRestaurant { get; set; }
}
