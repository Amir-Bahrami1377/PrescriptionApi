using FluentAssertions;
using Prescription.SharedKernel.Security;

namespace Prescription.SharedKernel.Tests;

public class AesColumnEncryptorTests
{
    private static AesColumnEncryptor Create(string key) => new(new ColumnEncryptionOptions { Key = key });

    private static string KeyOf(int bytes) => Convert.ToBase64String(new byte[bytes]);

    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(32)]
    public void Constructor_AcceptsEveryValidAesKeySize(int keyBytes)
    {
        var act = () => Create(KeyOf(keyBytes));

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(8)]
    [InlineData(20)]
    [InlineData(31)]
    [InlineData(64)]
    public void Constructor_RejectsAKeyOfTheWrongLength(int keyBytes)
    {
        // These are all perfectly valid Base64, which is why this used to slip past startup and
        // only surface on the first national code the application tried to encrypt.
        var act = () => Create(KeyOf(keyBytes));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*16, 24 or 32 bytes*")
            .WithMessage($"*decodes to {keyBytes}*");
    }

    [Fact]
    public void Constructor_RejectsSomethingThatIsNotBase64()
    {
        var act = () => Create("this is not base64!!");

        act.Should().Throw<InvalidOperationException>().WithMessage("*not valid Base64*");
    }

    [Fact]
    public void Constructor_NeverPutsTheKeyInTheErrorMessage()
    {
        const string secret = "c2VjcmV0LXRoYXQtbXVzdC1uZXZlci1sZWFr";

        var act = () => Create(secret);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().NotContain(secret);
    }

    [Fact]
    public void EncryptThenDecrypt_ReturnsTheOriginalValue()
    {
        var encryptor = Create(KeyOf(32));

        encryptor.Decrypt(encryptor.Encrypt("0499370899")).Should().Be("0499370899");
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var encryptor = Create(KeyOf(32));

        // A fresh nonce per call, so a repeated national code is not recognisable in the column.
        encryptor.Encrypt("0499370899").Should().NotBe(encryptor.Encrypt("0499370899"));
    }
}
