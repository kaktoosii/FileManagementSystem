
using Base.Common.Features.Identity;
using Base.DomainClasses;
using Base.ViewModels;

namespace Base.Services;

public interface IUsersService
{
    Task<string> GetSerialNumberAsync(int userId);
    Task<User> FindUserAsync(string username, string password);
    ValueTask<User> FindUserAsync(int userId);
    Task UpdateUserLastActivityDateAsync(int userId);
    ValueTask<User> GetCurrentUserAsync();
    int GetCurrentUserId();
    Task<(bool Succeeded, string Error)> ChangePasswordAsync(User user, string currentPassword, string newPassword);

    Task<List<UserDto>> GetAllUsersListAsync();
    Task<PagedResponse<List<UserViewModel>>> UserList(UserFilterDto filter);
    Task<User> AddUser(User user, List<Role> roles,List<int> cityIds);
    Task UpdateProfileImage(int userId, string profileImage);
    Task<User> FindUserByUserNameAsync(string username);
    Task UpdateProfileAsync(string userName, UserUpdateDto user);
    Task<int> RegisterUser(RegisterUserDto model);
    Task UpdateUserAsync(EditUserDto user);
    Task DeleteUserAsync(int userId);
    Task ConfirmCode(string userName);
    Task<List<string>> GetAllFcmTokenAsync();
    Task UpdateUserTokenFcmAsync(int userId, string token);
    Task<User> ConfirmUser(ConfirmCode model);
    Task<(bool Succeeded, string Error)> ForgetPasswordAsync(ChangePassword model);
    Task<UserDto> GetUserDtoAsync(int userId);
    Task<Role> getRole(string roleName);
    Task<int> getUserCount();
}