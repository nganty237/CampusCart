using System.Security.Cryptography;
using System.Text;

namespace CampusCart.Api.Modules.Auth;

public sealed class EmailVerificationCodeService
{
    private readonly byte[] _pepper;

    public EmailVerificationCodeService(string pepper)
    {
        if (string.IsNullOrWhiteSpace(pepper))
            throw new ArgumentException("A verification code pepper is required.", nameof(pepper));

        _pepper = Encoding.UTF8.GetBytes(pepper);
    }

    public string CreateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    public string Hash(string code) =>
        Convert.ToHexString(HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(code)));

    public bool Matches(string code, string expectedHash)
    {
        try
        {
            var expected = Convert.FromHexString(expectedHash);
            var actual = Convert.FromHexString(Hash(code));
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
