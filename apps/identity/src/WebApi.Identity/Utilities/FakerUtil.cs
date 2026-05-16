using System.Security.Cryptography;

namespace WebApi.Identity.Utilities;

public static class FakerUtil
{

    public static string GetRandomStringWithLength(int length)
    {
        ReadOnlySpan<char> allowedChars = "abcdefghklmnopqrstuvwxABCDEFGHIJKLMNOPQRSTUVWX";
        return RandomNumberGenerator.GetString(allowedChars, length);
    }
}
