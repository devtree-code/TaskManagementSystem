using TaskManagementSystem.BL.DTOs.Auth;

namespace TaskManagementSystem.BL.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}
