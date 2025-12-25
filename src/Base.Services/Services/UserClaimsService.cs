using Base.Common.Features.Identity;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.Services;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;

namespace Services.Services;

public class UserClaimsService : IUserClaimsService
{
    private readonly IApiActionsDiscoveryService _actionsDiscoveryService;
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;

    public UserClaimsService(
        ApplicationDbContext context,
        IApiActionsDiscoveryService actionsDiscoveryService,
        IUsersService usersService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actionsDiscoveryService =
            actionsDiscoveryService ?? throw new ArgumentNullException(nameof(actionsDiscoveryService));
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
    }

    public async Task AddOrUpdateUserClaimsAsync(int userId, string claimType, IReadOnlyList<string> inputClaimValues)
    {
        var user = await _context.Users
            .Include(user => user.UserClaims)
            .FirstOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return;
        }

        if (inputClaimValues is null || inputClaimValues.Count == 0)
        {
            user.UserClaims.Clear();
            await _context.SaveChangesAsync();
            return;
        }

        // Get current claims for the user and claim type
        var currentUserClaimValues = user.UserClaims
            .Where(userClaim => string.Equals(userClaim.ClaimType, claimType, StringComparison.Ordinal))
            .Select(userClaim => userClaim.ClaimValue)
            .ToList();

        // Calculate claims to add and remove
        var newClaimValuesToAdd = inputClaimValues.Except(currentUserClaimValues).ToList();
        var removedClaimValues = currentUserClaimValues.Except(inputClaimValues).ToList();

        // Remove claims that are no longer needed
        var claimsListToRemove = user.UserClaims
            .Where(userClaim => removedClaimValues.Contains(userClaim.ClaimValue)
                                && string.Equals(userClaim.ClaimType, claimType, StringComparison.Ordinal))
            .ToList();
        claimsListToRemove.ForEach(userClaim => user.UserClaims.Remove(userClaim));
        _context.UserClaims.RemoveRange(claimsListToRemove);

        // Add new claims
        foreach (var claimValue in newClaimValuesToAdd)
        {
            // Check if the claim exists in UserClaims
            var existingClaim = await _context.UserClaims
                .FirstOrDefaultAsync(uc => uc.ClaimType == claimType && uc.ClaimValue == claimValue);

            if (existingClaim == null)
            {
                // Create new claim if it doesn't exist
                existingClaim = new UserClaim
                {
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    Users = new List<User> { user }
                };
                _context.UserClaims.Add(existingClaim);
            }
            else
            {
                // Add user to existing claim's Users collection
                if (!existingClaim.Users.Any(u => u.Id == userId))
                {
                    existingClaim.Users.Add(user);
                    user.UserClaims.Add(existingClaim);
                }
            }
        }

        await _context.SaveChangesAsync();
    }
    // Other methods remain unchanged
    public Task<List<UserClaim>> GetUserClaimsAsync(int userId)
    {
        var userClaims = GetUserClaimsQuery(userId);
        return userClaims.ToListAsync();
    }

    public Task<List<UserClaim>> GetUserClaimsAsync(int userId, string claimType, string claimValue)
    {
        var userClaims = GetUserClaimsQuery(userId, claimType, claimValue);
        return userClaims.ToListAsync();
    }

    public async Task<ClaimsResponseDto> GetUserClaimsAsync(int userId, string claimType)
    {
        var userClaims = await GetUserClaimTypesAsync(userId, claimType) ?? new List<UserClaim>();
        var claimValues = userClaims.Select(userClaim => userClaim.ClaimValue).ToList();
        var user = await _context.Users
            .Where(usr => usr.Id == userId)
            .Select(usr => new UserDto
            {
                Id = usr.Id,
                Username = usr.Username,
                FirstName = usr.FirstName,
                LastName = usr.LastName
            }).FirstOrDefaultAsync();
        return new ClaimsResponseDto
        {
            User = user,
            ClaimValues = claimValues ?? new List<string>() // Ensure never null
        };
    }

    public Task<List<UserClaim>> GetUserClaimTypesAsync(int userId, string claimType)
    {
        var userClaims = GetUserClaimTypesQuery(userId, claimType);
        return userClaims.ToListAsync();
    }

    public Task<List<User>> GetUsersHaveUserClaimTypeAsync(string claimType)
    {
        var claimUserIdsQuery = from userClaim in _context.UserClaims
                                where userClaim.ClaimType == claimType
                                from user in userClaim.Users
                                select user.Id;
        return _context.Users.Where(user => claimUserIdsQuery.Contains(user.Id))
            .ToListAsync();
    }

    public Task<List<User>> GetUsersHaveUserClaimAsync(string claimType, string claimValue)
    {
        var claimUserIdsQuery = from userClaim in _context.UserClaims
                                where userClaim.ClaimType == claimType && userClaim.ClaimValue == claimValue
                                from user in userClaim.Users
                                select user.Id;
        return _context.Users.Where(user => claimUserIdsQuery.Contains(user.Id))
            .ToListAsync();
    }

    public async Task<bool> HasUserClaimTypeAsync(int userId, string claimType)
    {
        var userClaims = GetUserClaimTypesQuery(userId, claimType);
        var claim = await userClaims.FirstOrDefaultAsync();
        return claim != null;
    }

    public async Task<bool> HasUserClaimAsync(int userId, string claimType, string claimValue)
    {
        var userClaims = GetUserClaimsQuery(userId, claimType, claimValue);
        var claim = await userClaims.FirstOrDefaultAsync();
        return claim != null;
    }

    public Task<bool> HasCurrentUserClaimAsync(string claimType, string claimValue)
    {
        return HasUserClaimAsync(_usersService.GetCurrentUserId(), claimType, claimValue);
    }

    public async Task<SecuredActionsDto> GetActionsWithDynamicServerPermission(int userId)
    {
        var userClaims = await GetUserClaimsAsync(userId, CustomPolicies.DynamicServerPermission);
        return new SecuredActionsDto
        {
            DynamicallySecuredActions = _actionsDiscoveryService.DynamicallySecuredActions,
            UserClaims = userClaims
        };
    }

    private IQueryable<UserClaim> GetUserClaimTypesQuery(int userId, string claimType)
    {
        return from userClaim in _context.UserClaims
               where userClaim.ClaimType == claimType
               from user in userClaim.Users
               where user.Id == userId
               select userClaim;
    }

    private IQueryable<UserClaim> GetUserClaimsQuery(int userId, string claimType, string claimValue)
    {
        return from userClaim in _context.UserClaims
               where userClaim.ClaimType == claimType && userClaim.ClaimValue == claimValue
               from user in userClaim.Users
               where user.Id == userId
               select userClaim;
    }

    private IQueryable<UserClaim> GetUserClaimsQuery(int userId)
    {
        var userClaims = from userClaim in _context.UserClaims
                         from user in userClaim.Users
                         where user.Id == userId
                         select userClaim;
        return userClaims;
    }
}