using FoodFast.API.Domain;
using FoodFast.API.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace FoodFast.API.Services;

public interface ITokenGenerator
{
    string GenerateToken(ApplicationUser user);
}
