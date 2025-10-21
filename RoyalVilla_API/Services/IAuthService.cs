using RoyalVilla.DTO;

namespace RoyalVilla_API.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> RegisterAsync(RegisterationRequestDTO registerationRequestDTO);

        Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<bool> IsEmailExistsAsync(string email);
    }
}
