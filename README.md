# MILANO. Make It Low‑latency And Never Overfetch

A tiny, blazing‑fast **in‑memory key–value cache** written in **pure .NET** (no third‑party libs).  
Simple to reason about, predictable under concurrency, and optionally **sharded** to scale across CPU cores.

---

## Highlights

- **Pure .NET**: built on BCL only (ASP.NET Core), no external packages required.
- **Low latency path**: hot reads/writes backed by `ConcurrentDictionary`.
- **Per‑entry TTL**: optional expiration; a lightweight background cleaner removes expired keys.
- **Sharding**: split the store into N shards to reduce contention and scale with cores.
- **Straightforward API**: `Get`, `Set`, `Exists`, `Remove`, `Count`, `Dump`.
- **HTTP API**: clean, synchronous-friendly endpoints.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         MILANO Cache Server                         │
│                                                                     │
│  HTTP Endpoint (CacheController)  ────────────────►  ICacheService  │
│                                                                     │
│                                     ┌───────────────────────────┐   │
│                                     │   ShardedCacheService     │   │
│                                     │  (decorator over shards)  │   │
│                                     └─────────────┬─────────────┘   │
│         HashModuloShardingStrategy  ◄─────────────┘                 │
│                          ┌───────────────┬───────────────┬────────┐ │
│                          │    Shard 0    │    Shard 1    │  ...   │ │
│                          │ InMemoryCache │ InMemoryCache │        │ │
│                          └───────────────┴───────────────┴────────┘ │
│                                   ▲                                 │
│         ExpiredEntryCollection ◄──┘    BackgroundCleanupService     │
└─────────────────────────────────────────────────────────────────────┘
```

- **InMemoryCacheService** — holds `string` → `string` pairs with optional expiration.
- **ExpiredEntryCollection** — queue of keys with their expiration timestamps.
- **BackgroundCleanupService** — periodically evicts expired keys.
- **ShardedCacheService** — **decorator** that routes operations to one of N shards via `IShardingStrategy` (default: hash‑modulo).

---

## Quick start

### Requirements
- .NET 8 SDK

### Run the server
From the server project directory:
```bash
dotnet run
```
This hosts an **HTTP API** using ASP.NET Core Web API.

### Call from a .NET client (HttpClient)
```csharp
using var http = new HttpClient();
http.BaseAddress = new Uri("http://localhost:5000/cache");

var key = "hello";
var value = "world";

var content = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("key", key),
    new KeyValuePair<string, string>("value", value),
    new KeyValuePair<string, string>("expirationSeconds", "60")
});

await http.PostAsync("", content);

var result = await http.GetAsync(key);
Console.WriteLine(await result.Content.ReadAsStringAsync());
```

> MILANO currently stores and returns **string values** (so “what you put is what you get”).

---

## HTTP endpoints

- `POST /cache` — set a key
- `GET /cache/{key}` — get a value
- `DELETE /cache/{key}` — remove a key
- `HEAD /cache/{key}` — check existence
- `GET /cache/count` — get item count
- `GET /cache/dump?includeExpired=true` — dump all keys

All endpoints support `x-api-key` for optional key validation.

---

## Configuration

Use `appsettings.json`:

```jsonc
{
  "Cache": {
    "MaxPayloadSizeBytes": 1000000,
    "DefaultExpirationSeconds": null,
    "EnableAutoCleanup": true
  },
  "Sharding": {
    "Enabled": true,
    "ShardCount": 4
  }
}
```

Dependency injection (excerpt):
```csharp
services.AddSingleton<ExpiredEntryCollection>();
services.AddSingleton<IShardingStrategy, HashModuloShardingStrategy>();

services.AddSingleton<ICacheService>(sp =>
{
    var expired = sp.GetRequiredService<ExpiredEntryCollection>();
    var strategy = sp.GetRequiredService<IShardingStrategy>();
    return new ShardedCacheService(
        4,
        shardIndex => new InMemoryCacheService(expired, maxPayloadSizeBytes: 1_000_000),
        strategy
    );
});

services.AddHostedService<BackgroundCleanupService>();
```

---

## Usage notes

- **TTL**: pass `expirationSeconds` when setting a key.
- **Count()**: returns non‑expired keys only.
- **Dump(includeExpired: true)**: debug dump that also includes expired entries.
- **Sharding**: default strategy is `HashModuloShardingStrategy`.

---

## Benchmarks

On a 6C/12T machine, MILANO reached **37k+ req/s** over HTTP with `p99 < 1.6ms` in local benchmarks.  
Requests used real `HttpClient`, not in‑process fakes.

> Run the bundled CLI benchmark tool for stress tests and latency stats.

---

## Internals

```csharp
public interface ICacheService
{
    Task<CacheResponse> GetAsync(CacheGetRequest request);
    Task SetAsync(CacheSetRequest request);
    Task<bool> ExistsAsync(string key);
    Task<bool> RemoveAsync(string key);
    Task<int> CountAsync();
    Task<IDictionary<string, string>> DumpAsync(bool includeExpired = false);
}
```

---

## Roadmap

- Optional **binary payloads** (`Content-Type`, raw bytes).
- Raw GET for file-like usage.
- Admin dashboard for API keys.
- Custom binary protocol - MCP: MILANO Cache Protocol.


---

## License

Licensed under the MIT License.

**MILANO. Make It Low‑latency And Never Overfetch.**
