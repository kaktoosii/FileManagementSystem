using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base;
public static class RandomStringGenerator
{
    private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateSecureAlphanumericId(int length = 16)
    {
        if (length < 11)
            throw new ArgumentException("Length must be at least 8 characters.");

        // Create a byte array to hold the random bytes
        byte[] randomBytes = new byte[length];

        // Fill the array with cryptographically secure random bytes
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert the random bytes to an alphanumeric string
        var result = new StringBuilder(length);
        foreach (byte b in randomBytes)
        {
            // Use modulo to ensure the index is within the bounds of AlphanumericChars
            result.Append(AlphanumericChars[b % AlphanumericChars.Length]);
        }

        return result.ToString();
    }
}
