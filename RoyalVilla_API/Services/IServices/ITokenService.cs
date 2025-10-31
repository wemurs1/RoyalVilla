using RoyalVilla_API.Models;

namespace RoyalVilla_API.Services.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
