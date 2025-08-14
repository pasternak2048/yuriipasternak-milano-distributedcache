# MILANO. Make It Low‑latency And Never Overfetch

A tiny, blazing‑fast **in‑memory key–value cache** written in **pure .NET** (no third‑party libs).  
Simple to reason about, predictable under concurrency, and optionally **sharded** to scale across CPU cores.

---

## Highlights

- **Pure .NET**: built on BCL only (ASP.NET Core + gRPC), no external packages required.
- **Low latency path**: hot reads/writes backed by `ConcurrentDictionary`.
- **Per‑entry TTL**: optional expiration; a lightweight background cleaner removes expired keys.
- **Sharding**: split the store into N shards to reduce contention and scale with cores.
- **Straightforward API**: `Get`, `Set`, `Exists`, `Remove`, `Count`, `Dump`.
- **gRPC service**: easy to integrate from .NET or any gRPC‑capable client.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         MILANO Cache Server                         │
│                                                                     │
│  gRPC Endpoint (CacheService)  ────────────────►  ICacheService     │
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
This hosts a **gRPC** endpoint (Kestrel).

### Call from a .NET client (gRPC)
```csharp
using Grpc.Net.Client;
using Grpc.Core;
using MILANO.DistributedCache.Server.Web.Grpc;

var channel = GrpcChannel.ForAddress("http://localhost:5000"); // adjust if needed
var client  = new CacheService.CacheServiceClient(channel);

// If API key validation is enabled, pass the header (example):
var headers = new Metadata { { "x-api-key", "<your key>" } };

// SET
await client.SetAsync(new GrpcCacheSetRequest {
    Key = "hello",
    Value = "world",
    ExpirationSeconds = 60,
    ApiKey = "<your key>"
}, headers);

// GET
var resp = await client.GetAsync(new GrpcCacheGetRequest {
    Key = "hello",
    ApiKey = "<your key>"
}, headers);

Console.WriteLine($"{resp.Key} = {resp.Value} (found: {resp.Found})");
```

> MILANO currently stores and returns **string values** (so “what you put is what you get”).

---

## gRPC contract (string values)

```proto
syntax = "proto3";
option csharp_namespace = "MILANO.DistributedCache.Server.Web.Grpc";

service CacheService {
  rpc Get    (GrpcCacheGetRequest)    returns (GrpcCacheGetResponse);
  rpc Set    (GrpcCacheSetRequest)    returns (GrpcCacheSetResponse);
  rpc Remove (GrpcCacheRemoveRequest) returns (GrpcCacheRemoveResponse);
  rpc Exists (GrpcCacheExistsRequest) returns (GrpcCacheExistsResponse);
  rpc Count  (GrpcCacheCountRequest)  returns (GrpcCacheCountResponse);
  rpc Dump   (GrpcCacheDumpRequest)   returns (GrpcCacheDumpResponse);
}

message GrpcCacheGetRequest  { string key = 1; string apiKey = 2; }
message GrpcCacheGetResponse { string key = 1; string value = 2; bool found = 3; }

message GrpcCacheSetRequest  { string key = 1; string value = 2; string apiKey = 3; int32 expirationSeconds = 4; }
message GrpcCacheSetResponse { bool success = 1; }

message GrpcCacheRemoveRequest { string key = 1; string apiKey = 2; }
message GrpcCacheRemoveResponse{ bool success = 1; }

message GrpcCacheExistsRequest{ string key = 1; string apiKey = 2; }
message GrpcCacheExistsResponse{ bool exists = 1; }

message GrpcCacheCountRequest { string apiKey = 1; }
message GrpcCacheCountResponse{ int32 count = 1; }

message GrpcCacheDumpRequest  { bool includeExpired = 1; string apiKey = 2; }
message GrpcCacheDumpResponse { map<string, string> entries = 1; }
```

---

## Configuration

Use `appsettings.json`:

```jsonc
{
  "Cache": {
    "MaxPayloadSizeBytes": 1000000,        // ~1 MB value limit
    "DefaultExpirationSeconds": null,      // null = no default TTL
    "EnableAutoCleanup": true              // background cleaner
  },
  "Sharding": {
    "Enabled": true,
    "ShardCount": 4                        // typically near logical CPU count
  }
}
```

Dependency injection (excerpt):
```csharp
services.AddMemoryCache();
services.AddSingleton<ExpiredEntryCollection>();
services.AddSingleton<IShardingStrategy, HashModuloShardingStrategy>();

services.AddSingleton<ICacheService>(sp =>
{
    var expired  = sp.GetRequiredService<ExpiredEntryCollection>();
    var strategy = sp.GetRequiredService<IShardingStrategy>();

    // In a real app read from IConfiguration/IOptions<>
    var shardCount = 4;

    return new ShardedCacheService(
        shardCount,
        shardIndex => new InMemoryCacheService(expired, maxPayloadSizeBytes: 1_000_000),
        strategy
    );
});

services.AddHostedService<BackgroundCleanupService>();
services.AddGrpc();
```

---

## Usage notes

- **TTL**: pass `ExpirationSeconds` in `Set`. `0`/`null` means “no expiration”.
- **Count()**: returns the number of **non‑expired** entries.
- **Dump(includeExpired: true)**: debug dump that also includes expired entries.
- **Sharding**: default strategy is `HashModuloShardingStrategy` based on `GetHashCode(key) % ShardCount`.

---

## Benchmarks (very short)

On a typical 6C/12T machine, a single MILANO instance reached **~25k req/s** with **p99 ≈ 1.2 ms** under concurrent load in local tests.  
(Results vary by hardware, runtime config, payload sizes, and network stack.)

> A small benchmark app is included to run smoke/burst/soak tests and print latency/RPS summaries.

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
- `InMemoryCacheService` stores `string` values with optional `DateTimeOffset? Expiration` per entry.
- `BackgroundCleanupService` checks `ExpiredEntryCollection` and evicts expired keys regularly.
- `ShardedCacheService` aggregates multiple `ICacheService` backends and routes keys to a shard.

---

## Roadmap

- Optional **binary values** with `ContentType` (bytes + mimetype).
- Raw download endpoint for binary payloads.
- Admin panel for api keys management.

---

## License

Licensed under the [MIT License](LICENSE).

**MILANO. Make It Low‑latency And Never Overfetch.**
