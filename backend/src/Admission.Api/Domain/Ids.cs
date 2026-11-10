using System.Security.Cryptography;

namespace Admission.Api.Domain;

public static class Ids
{
    public static string NewClassId() => "c_" + RandomPart();
    public static string NewHoldId() => "h_" + RandomPart();

    // 16 hex chars = 64 random bits: short enough for URLs, collisions practically impossible.
    private static string RandomPart() => Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
}
