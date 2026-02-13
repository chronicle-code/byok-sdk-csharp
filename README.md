# BYOK SDK for C# / Unity

Official C# SDK for the [BYOK](https://byok.gg) platform. Targets **.NET Standard 2.1** for Unity 2021+ compatibility.

## Installation

### Unity (via .dll)

1. Build the project: `dotnet build -c Release`
2. Copy `Byok/bin/Release/netstandard2.1/Byok.dll` into your Unity project's `Assets/Plugins/` folder
3. Also copy `System.Text.Json.dll` and `Microsoft.Bcl.AsyncInterfaces.dll` from the same output directory

### NuGet

```
dotnet add package Byok
```

## Quick Start

```csharp
using Byok;
using Byok.Models;

// Create a client
var client = new ByokClient("byok_sk_live_your_key_here");

// Send a chat message
var response = await client.Chat.SendAsync(new ChatRequest
{
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "What do you have for sale?" }
    },
    LorevaultEntityRef = "characters/blacksmith-thorn",
    QualityTier = QualityTier.Standard,
    UserId = "player_123",
    GameState = new Dictionary<string, object> { { "location", "market_square" } }
});

Console.WriteLine(response.Choices[0].Message.Content);
```

## Streaming

```csharp
var request = new ChatRequest
{
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Tell me about this dungeon." }
    },
    UserId = "player_123"
};

await foreach (var chunk in client.Chat.StreamAsync(request))
{
    if (!chunk.Done)
        Console.Write(chunk.Content);
}
```

## Player Registration

The SDK supports all three authentication methods:

```csharp
// Steam
var result = await client.Players.RegisterAsync(new SteamRegistration
{
    SteamId = "76561198000000000",
    DisplayName = "SteamPlayer"
});

// Anonymous device
var result2 = await client.Players.RegisterAsync(new DeviceRegistration
{
    DeviceId = "550e8400-e29b-41d4-a716-446655440000"
});

// External provider (Epic, custom auth, etc.)
var result3 = await client.Players.RegisterAsync(new ExternalRegistration
{
    ExternalId = "player_abc",
    ExternalProvider = "epic",
    DisplayName = "EpicPlayer"
});

Console.WriteLine($"User ID: {result.UserId}, Created: {result.Created}");
```

## Events

```csharp
// Ingest a game event
var ingestResult = await client.Events.IngestAsync(new IngestEventRequest
{
    EventType = "world.discovery",
    Payload = new Dictionary<string, object>
    {
        { "area", "hidden_cave" },
        { "discovered_by", "player_123" }
    }
}, userId: "player_123");

// Poll for events
var events = await client.Events.PollAsync(userId: "player_123");
foreach (var evt in events.Events)
{
    Console.WriteLine($"{evt.EventType}: {evt.Id}");
    // Acknowledge
    await client.Events.AckAsync(evt.Id, userId: "player_123");
}

// Batch acknowledge
await client.Events.AckBatchAsync(
    new List<string> { "evt_1", "evt_2" },
    userId: "player_123"
);
```

## Player State

```csharp
// Get current state
var state = await client.State.GetAsync(userId: "player_123");
Console.WriteLine($"Location: {state.Location}");

// Update state
var updated = await client.State.UpdateAsync(new UpdateStateRequest
{
    Location = "dark_forest",
    MarkersActivated = new List<string> { "forest_entrance" },
    Custom = new Dictionary<string, object> { { "torch_lit", true } }
}, userId: "player_123");
```

## Wallet

```csharp
var wallet = await client.Wallet.GetAsync(userId: "player_123");
Console.WriteLine($"Balance: {wallet.AvailableCents} cents ({wallet.Currency})");
```

## Player Intelligence

```csharp
// Behavioral profile
var profile = await client.Players.GetProfileAsync("player_123");
Console.WriteLine($"Curiosity: {profile.Dimensions.Curiosity}");

// Highlight reel
var highlights = await client.Players.GetHighlightsAsync("player_123",
    new HighlightParams { Limit = 10 });

// Spending intelligence
var spending = await client.Players.GetSpendingAsync("player_123");

// Cross-game passport
var passport = await client.Players.GetPassportAsync("player_123");
await client.Players.SetPassportConsentAsync(
    new PassportConsentRequest { Consent = true },
    userId: "player_123"
);
```

## Error Handling

All API errors throw typed exceptions inheriting from `ByokException`:

```csharp
using Byok.Errors;

try
{
    await client.Chat.SendAsync(request);
}
catch (RateLimitException ex)
{
    Console.WriteLine($"Rate limited. Retry after {ex.RetryAfter} seconds.");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Auth failed: {ex.Message}");
}
catch (PaymentRequiredException ex)
{
    Console.WriteLine($"Insufficient funds: {ex.Message}");
}
catch (ByokException ex)
{
    Console.WriteLine($"API error {ex.StatusCode}: {ex.Message}");
}
```

| Exception | HTTP Status | When |
|---|---|---|
| `AuthenticationException` | 401 | Invalid or missing API key |
| `PaymentRequiredException` | 402 | No wallet or insufficient balance |
| `ForbiddenException` | 403 | Game not active, tier unavailable |
| `NotFoundException` | 404 | Resource not found |
| `ValidationException` | 422 | Invalid request parameters |
| `RateLimitException` | 429 | Rate limited (check `RetryAfter`) |
| `ProviderException` | 502 | Upstream AI provider error |

## Advanced: Custom HttpClient

For Unity or DI scenarios, you can pass your own `HttpClient`:

```csharp
var httpClient = new HttpClient();
var client = new ByokClient(new ByokClientOptions
{
    ApiKey = "byok_sk_...",
    HttpClient = httpClient,
    BaseUrl = "https://byok.gg"
});
```

When you provide your own `HttpClient`, `ByokClient.Dispose()` will not dispose it.

## Requirements

- .NET Standard 2.1 (Unity 2021.2+, .NET Core 3.0+, .NET 5+)
- System.Text.Json 6.0+
- Microsoft.Bcl.AsyncInterfaces 6.0+ (for `IAsyncEnumerable` on .NET Standard 2.1)
