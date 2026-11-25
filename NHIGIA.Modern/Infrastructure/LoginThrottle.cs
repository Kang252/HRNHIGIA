using Microsoft.Extensions.Caching.Memory;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>
/// Slows password guessing. Keyed by account and address together so nobody can lock a colleague out from elsewhere,
/// with a wider limit per address. State is per process, which matches the single web instance.
/// </summary>
public sealed class LoginThrottle(IMemoryCache cache)
{
    public const int MaxPerAccount = 5;
    public const int MaxPerAddress = 40;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
    private readonly Lock _gate = new();

    public TimeSpan? RetryAfter(string username, string address)
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            var blocked = new[] { (Key: AccountKey(username, address), Max: MaxPerAccount), (Key: AddressKey(address), Max: MaxPerAddress) }
                .Select(x => cache.TryGetValue(x.Key, out Attempts attempts) && attempts.Count >= x.Max ? attempts.Until - now : TimeSpan.Zero)
                .Max();
            return blocked > TimeSpan.Zero ? blocked : null;
        }
    }

    public void Fail(string username, string address)
    {
        lock (_gate)
        {
            Increment(AccountKey(username, address));
            Increment(AddressKey(address));
        }
    }

    public void Succeed(string username, string address)
    {
        lock (_gate) cache.Remove(AccountKey(username, address));
    }

    private void Increment(string key)
    {
        var until = DateTimeOffset.UtcNow.Add(Window);
        var count = cache.TryGetValue(key, out Attempts attempts) ? attempts.Count + 1 : 1;
        cache.Set(key, new Attempts(count, until), until);
    }

    private static string AccountKey(string username, string address) => $"login:{username?.Trim().ToLowerInvariant()}|{address}";
    private static string AddressKey(string address) => $"login-address:{address}";

    private sealed record Attempts(int Count, DateTimeOffset Until);
}
