using RoyalVilla_API.Models;
using System.Security.Claims;

namespace RoyalVilla_API.Services
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
