using FoodFast.API.Domain.Models;


namespace FoodFast.API.Services;

public interface IEmailService
{
    Task SendAsync(EmailRequest request);
}
