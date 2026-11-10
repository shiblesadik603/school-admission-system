using System.Security.Cryptography;

namespace Admission.Api.Helpers;

public static class IdGenerator
{
    // e.g. c_3f9a1b2c4d5e6f70 / h_0a1b2c3d4e5f6a7b
    public static string NewClassId() => "c_" + RandomHex();
    public static string NewHoldId() => "h_" + RandomHex();

    private static string RandomHex()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
    }
}
