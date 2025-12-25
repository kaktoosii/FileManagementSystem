using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Base.Common.Features.Identity;
using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.ViewModels;
using CoordinateSharp;
using DNTPersianUtils.Core;
using DNTPersianUtils.Core.IranCities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;

namespace Base.Services;

public class UsersService : IUsersService
{
    private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ISecurityService _securityService;
    private readonly IUnitOfWork _uow;
    private readonly DbSet<User> _users;
    private readonly DbSet<Role> _role;
    private readonly DbSet<UserRole> _userRole;
    private readonly DbSet<UserLocation> _userLocation;
    private readonly ISmsSender _smsSender;

    public UsersService(IUnitOfWork uow, ISecurityService securityService, IHttpContextAccessor contextAccessor, ISmsSender smsSender)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _users = _uow.Set<User>();
        _role = _uow.Set<Role>();
        _userRole = _uow.Set<UserRole>();
        _userLocation = _uow.Set<UserLocation>();
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _smsSender = smsSender;
    }

    public ValueTask<User> FindUserAsync(int userId) => _users.FindAsync(userId);

    public Task<User> FindUserAsync(string username, string password)
    {
        var passwordHash = _securityService.GetSha256Hash(password);

        return _users.FirstOrDefaultAsync(x => x.Username == username && x.Password == passwordHash);
    }

    public async Task<string> GetSerialNumberAsync(int userId)
    {
        var user = await FindUserAsync(userId);

        return user?.SerialNumber;
    }

    public async Task UpdateUserLastActivityDateAsync(int userId)
    {
        var user = await FindUserAsync(userId);

        if (user is null)
        {
            return;
        }

        if (user.LastLoggedIn != null)
        {
            var updateLastActivityDate = TimeSpan.FromMinutes(value: 2);
            var currentUtc = DateTimeOffset.UtcNow;
            var timeElapsed = currentUtc.Subtract(user.LastLoggedIn.Value);

            if (timeElapsed < updateLastActivityDate)
            {
                return;
            }
        }

        user.LastLoggedIn = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync();
    }

    public int GetCurrentUserId()
    {
        var claimsIdentity = _contextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        var userId = userDataClaim?.Value;

        return string.IsNullOrWhiteSpace(userId)
            ? 0
            : int.Parse(userId, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public ValueTask<User> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();

        return FindUserAsync(userId);
    }

    public async Task<(bool Succeeded, string Error)> ChangePasswordAsync(User user,
        string currentPassword,
        string newPassword)
    {
        ArgumentNullException.ThrowIfNull(user);

        var currentPasswordHash = _securityService.GetSha256Hash(currentPassword);

        if (!string.Equals(user.Password, currentPasswordHash, StringComparison.Ordinal))
        {
            return (false, "Current password is wrong.");
        }

        user.Password = _securityService.GetSha256Hash(newPassword);

        // user.SerialNumber = Guid.NewGuid().ToString("N"); // To force other logins to expire.
        await _uow.SaveChangesAsync();

        return (true, string.Empty);
    }










    public async Task<PagedResponse<List<UserViewModel>>> UserList(UserFilterDto filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        UserFilterDto validFilter = new(pageNumber: filter.PageNumber, filter.PageSize, filter.FirstName, filter.LastName, filter.UserName, filter.SearchText);
        var list = _users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(validFilter.UserName))
        {
            list = list.Where(x => x.Username.Contains(validFilter.UserName));
        }
        if (!string.IsNullOrWhiteSpace(validFilter.FirstName))
        {
            list = list.Where(x => x.FirstName.Contains(validFilter.FirstName));
        }
        if (!string.IsNullOrWhiteSpace(validFilter.LastName))
        {
            list = list.Where(x => x.LastName.Contains(validFilter.LastName));
        }
        if (!string.IsNullOrWhiteSpace(validFilter.SearchText))
        {
            list = list.Where(x => x.Username.Contains(validFilter.SearchText) || x.FirstName.Contains(validFilter.FirstName) || x.LastName.Contains(validFilter.LastName));
        }
        var totalRecords = await list.Select(x => x.Id).CountAsync();
        var pagedData = await list
            .Select(user => new UserViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.Username,
                MobileNumber = user.MobileNumber,
                IsCheckDistance = user.IsCheckDistance,
                Distance = user.Distance,
                DeviceId = user.DeviceId
            })
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();

        return new PagedResponse<List<UserViewModel>>(pagedData, validFilter.PageNumber, validFilter.PageSize, totalRecords);
    }


    public async Task<(bool Succeeded, string Error)> ForgetPasswordAsync(ChangePassword model)
    {
        if (model is null)
        {
            return (false, "Model is null.");
        }
        if (model.NewPassword.Length < 8)
            throw new AppException("رمز عبور باید حداقل 8 حرف باشد.");
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(model.Token);
        var userName = jwtSecurityToken.Claims.First(claim => string.Equals(claim.Type, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", StringComparison.Ordinal)).Value;
        var user = await FindUserByUserNameAsync(userName);
        if (user == null)
        {
            return (false, "User not found.");
        }

        user.Password = _securityService.GetSha256Hash(model.NewPassword);
        await _uow.SaveChangesAsync();
        return (true, string.Empty);
    }
    public Task<List<UserDto>> GetAllUsersListAsync()
    {
        return _users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            IsCheckDistance = user.IsCheckDistance,
            Distance = user.Distance,
            DeviceId = user.DeviceId
        }).ToListAsync();
    }
    public Task<UserDto> GetUserDtoAsync(int userId)
    {
        return _users.Where(user => user.Id == userId).Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            IsCheckDistance = user.IsCheckDistance,
            Distance = user.Distance,
            DeviceId = user.DeviceId
        }).FirstOrDefaultAsync();
    }
    public async Task<User> FindUserByUserNameAsync(string username)
    {
        return await _users.FirstOrDefaultAsync(x => x.Username == username);
    }
    public static bool CheckUserName(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new AppException("نام کاربری نمی تواند خالی باشد.");
        if (username.Length < 8)
            throw new AppException("نام کاربری باید حداقل 8 حرف باشد.");
        if (username.ContainsFarsi())
            throw new AppException("نام کاربری فقط با حروف و اعداد انگلیسی وارد کنید.");
        return true;
    }
    public List<User> UserInRole(List<int> roles)
    {
        return _users.Where(applicationUser => roles.Contains(applicationUser.Id))
                    .ToList();
    }

    public async Task<int> RegisterUser(RegisterUserDto model)
    {
        if (model == null)
            throw new AppException("مقادیر نباید خالی باشد");
        CheckUserName(model.Username);
        var userExisted = await FindUserByUserNameAsync(model.Username);
        if (userExisted != null)
            throw new AmbiguousMatchException("این نام کاربری موجود است");


        var adminRole = new Role
        {
            Name = CustomRoles.Editor
        };

        var userRole = new Role
        {
            Name = CustomRoles.User
        };
        if (!await _role.AnyAsync())
        {
            _role.Add(adminRole);
            _role.Add(userRole);
            await _uow.SaveChangesAsync();
        }
        List<Role> roles = new List<Role>();
        roles.Add(adminRole);
        roles.Add(userRole);
        var user = new User()
        {
            Username = model.Username,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MobileNumber = model.MobileNumber,
            IsCheckDistance = model.IsCheckDistance,
            Distance = model.Distance,
            DeviceId = model.DeviceId
        };
        user = await AddUser(user, roles,new List<int>());
        return user.Id;
    }
    public async Task<User> AddUser(User user, List<Role> roles,List<int> cityIds)
    {
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(cityIds);

        var insertedUser = new User
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.FirstName + " " + user.LastName,
            ProfileImage = user.ProfileImage,
            MobileNumber = user.MobileNumber,
            IsActive = true,
            LastLoggedIn = null,
            IsCheckDistance = user.IsCheckDistance,
            Distance = user.Distance,
            Password = _securityService.GetSha256Hash(user.Password),
            SerialNumber = Guid.NewGuid().ToString(format: "N")
        };
        if (await FindUserByUserNameAsync(user.Username) == null)
        {
            _users.Add(insertedUser);
            foreach (var role in roles)
            {
                var roleModel = await getRole(role.Name);
                if(roleModel != null)
                {
                    await _userRole.AddAsync(new UserRole
                    {
                        Role = roleModel,
                        User = insertedUser
                    });
                }
                else
                {
                    await _userRole.AddAsync(new UserRole
                    {
                        Role = role,
                        User = insertedUser
                    });
                }
               
            }
            foreach (var city in cityIds)
            {
                await _userLocation.AddAsync(new UserLocation
                {
                    CityId = city,
                    User = insertedUser
                });
            }
        }
        await _uow.SaveChangesAsync();
        return insertedUser;
    }
    public async Task UpdateProfileImage(int userId, string profileImage)
    {
        var user = await _users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("یافت نشد");
        user.ProfileImage = profileImage;
        await _uow.SaveChangesAsync();

    }

    public async Task UpdateProfileAsync(string userName, UserUpdateDto user)
    {
        if (user == null)
            throw new KeyNotFoundException("یافت نشد");
        var model = await FindUserByUserNameAsync(userName);
        model.FirstName = user.FirstName;
        model.LastName = user.LastName;
        model.MobileNumber = user.MobileNumber;
        model.DisplayName = user.DisplayName;
        model.ProfileImage = user.ProfileImage;
        model.IsCheckDistance = user.IsCheckDistance;
        model.Distance = user.Distance;
        model.DeviceId = user.DeviceId;
        await _uow.SaveChangesAsync();
    }
    public async Task UpdateUserAsync(EditUserDto user)
    {
        if (user == null)
            throw new KeyNotFoundException("یافت نشد");
        var model = await _users.FirstOrDefaultAsync(u=>u.Id == user.Id);
        if (model == null)
            throw new KeyNotFoundException("یافت نشد");
        model.FirstName = user.FirstName;
        model.LastName = user.LastName;
        model.Username = user.Username;
        model.MobileNumber = user.MobileNumber;
        model.ProfileImage = user.ProfileImage;
        model.DisplayName = user.FirstName + " " + user.LastName;
        model.IsCheckDistance = user.IsCheckDistance;
        model.Distance = user.Distance;
        model.DeviceId = user.DeviceId;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("یافت نشد");
        try
        {
            _users.Remove(user);
            await _uow.SaveChangesAsync();
        }
        catch
        {
            throw new KeyNotFoundException("امکان حذف این کاربر وجود ندارد.");
        }
    }
    public async Task<Role> getRole(string roleName)
    {
       Role role =await _role.FirstOrDefaultAsync(x => x.Name.Contains(roleName));
        if (role == null)
            return null;
        else
            return role;
    }
    public async Task<int> getUserCount()
    {
        return await _users.CountAsync();
    }
    public async Task ConfirmCode(string userName)
    {
        var user = await FindUserByUserNameAsync(userName);
        if (user == null)
            throw new KeyNotFoundException("یافت نشد");
        user.Confirmcode = 12345;//Next(10000, 99999);
        user.ConfirmcodeDate = DateTime.Now.AddMinutes(2);
        await _uow.SaveChangesAsync();
        string message = user.Confirmcode.ToString(CultureInfo.CurrentCulture);
        if (string.IsNullOrWhiteSpace(user.MobileNumber))
            throw new KeyNotFoundException("شماره موبایل برای کاربری شما تعریف نشده است");
        await _smsSender.SendSmsAsync(user.MobileNumber, message);
    }

    public async Task<List<string>> GetAllFcmTokenAsync()
    {
        return await _users.Where(x => x.FcmToken != null).Select(x => x.FcmToken).ToListAsync();
    }

    public async Task UpdateUserTokenFcmAsync(int userId, string token)
    {
        var user = await _users.FindAsync(userId);
        user.FcmToken = token;
        await _uow.SaveChangesAsync();
    }
    public async Task<User> ConfirmUser(ConfirmCode model)
    {
        var user = await _users.FirstOrDefaultAsync(x => x.Confirmcode == model.ConfirmCodeNumber && x.Username == model.Username && x.ConfirmcodeDate > DateTime.UtcNow);
        if (user == null)
            throw new AppException("کد وارد شده نامعتبر است");
        return user;
    }
    public int Next()
    {
        var randb = new byte[4];
        _rand.GetBytes(randb);
        var value = BitConverter.ToInt32(randb, 0);
        if (value < 0) value = -value;
        return value;
    }

    public int Next(int max)
    {
        var randb = new byte[4];
        _rand.GetBytes(randb);
        var value = BitConverter.ToInt32(randb, 0);
        value = value % (max + 1); // % calculates remainder
        if (value < 0) value = -value;
        return value;
    }

    public int Next(int min, int max)
    {
        var value = Next(max - min) + min;
        return value;
    }
}