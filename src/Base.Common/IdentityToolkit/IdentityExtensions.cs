using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Common.IdentityToolkit;

public static class IdentityExtensions
{
    public static void AddErrorsFromResult(this ModelStateDictionary modelStat, IdentityResult result)
    {
        if (result == null || modelStat == null)
        {
            throw new ArgumentNullException(nameof(result));
        }
        foreach (var error in result.Errors)
        {
            modelStat.AddModelError("", error.Description);
        }
    }

    /// <summary>
    /// IdentityResult errors list to string
    /// </summary>
    public static string DumpErrors(this IdentityResult result, bool useHtmlNewLine = false)
    {
        var results = new StringBuilder();
        ArgumentNullException.ThrowIfNull(result);
        if (result.Succeeded)
        {
            return results.ToString();
        }

        foreach (var errorDescription in from error in result.Errors
                                         let errorDescription = error.Description
                                         select errorDescription)
        {
            if (string.IsNullOrWhiteSpace(errorDescription))
            {
                continue;
            }

            if (!useHtmlNewLine)
            {
                results.AppendLine(errorDescription);
            }
            else
            {
                results.Append(errorDescription).AppendLine("<br/>");
            }
        }

        return results.ToString();
    }

    public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
    {
        return identity?.FindFirst(claimType)?.Value;
    }

    public static string GetUserClaimValue(this IIdentity identity, string claimType)
    {
        var identity1 = identity as ClaimsIdentity;
        return identity1?.FindFirstValue(claimType);
    }

    public static string GetUserFirstName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue(ClaimTypes.GivenName);
    }

    public static string GetUserEmail(this IIdentity identity)
    {
        return identity?.GetUserClaimValue(ClaimTypes.Email);
    }
    public static T GetUserId<T>(this IIdentity identity) where T : IConvertible
    {
        var firstValue = identity?.GetUserClaimValue(ClaimTypes.NameIdentifier);
        return firstValue != null
            ? (T)Convert.ChangeType(firstValue, typeof(T), CultureInfo.InvariantCulture)
            : default(T);
    }

    public static int GetUserId(this IIdentity identity)
    {
        return int.Parse(identity?.GetUserClaimValue(ClaimTypes.NameIdentifier), CultureInfo.InvariantCulture);
    }

    public static string GetUserLastName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue(ClaimTypes.Surname);
    }

    public static string GetUserFullName(this IIdentity identity)
    {
        return $"{GetUserFirstName(identity)} {GetUserLastName(identity)}";
    }
    public static string GetUserMobileNumber(this IIdentity identity)
    {
        return identity?.GetUserClaimValue(ClaimTypes.MobilePhone); 
    }

    public static string GetUserDisplayName(this IIdentity identity)
    {
        var fullName = GetUserFullName(identity);
        return string.IsNullOrWhiteSpace(fullName) ? GetUserName(identity) : fullName;
    }

    public static string GetUserName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue(ClaimTypes.Name);
    }
    public static int GetClinicId(this IIdentity identity)
    {
        return int.Parse(identity?.GetUserClaimValue("ClinicId"), CultureInfo.InvariantCulture);
    }
}