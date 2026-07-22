using StackExchange.Redis;

namespace Prescription.Modules.Identity.Infrastructure.Otp;

public sealed class RedisOtpCodeStore(IConnectionMultiplexer redis) : IOtpCodeStore
{
    private IDatabase Database => redis.GetDatabase();

    public Task SaveCodeAsync(string phoneNumber, string code, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        Database.StringSetAsync(CodeKey(phoneNumber), code, ttl);

    public async Task<string?> GetCodeAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(CodeKey(phoneNumber));
        return value.HasValue ? value.ToString() : null;
    }

    public Task RemoveCodeAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        Database.KeyDeleteAsync(CodeKey(phoneNumber));

    public async Task<int> IncrementRequestCountAsync(string phoneNumber, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var key = RateLimitKey(phoneNumber);
        var count = await Database.StringIncrementAsync(key);
        if (count == 1)
        {
            await Database.KeyExpireAsync(key, window);
        }

        return (int)count;
    }

    private static string CodeKey(string phoneNumber) => $"otp:code:{phoneNumber}";

    private static string RateLimitKey(string phoneNumber) => $"otp:rate:{phoneNumber}";
}
