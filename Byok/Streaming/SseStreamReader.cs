using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Byok.Models;

namespace Byok.Streaming
{
    /// <summary>
    /// Parses a server-sent events stream from a BYOK streaming chat response
    /// into an IAsyncEnumerable of ChatStreamChunk.
    /// </summary>
    public static class SseStreamReader
    {
        public static async IAsyncEnumerable<ChatStreamChunk> ReadAsync(
            Stream stream,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                if (!trimmed.StartsWith("data: ")) continue;
                var data = trimmed.Substring(6);

                if (data == "[DONE]")
                {
                    yield return new ChatStreamChunk { Content = "", Done = true };
                    yield break;
                }

                JsonDocument? doc = null;
                try
                {
                    doc = JsonDocument.Parse(data);
                }
                catch
                {
                    // Non-JSON SSE line (comments, keep-alive) -- skip
                    continue;
                }

                using (doc)
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("choices", out var choices) &&
                        choices.GetArrayLength() > 0)
                    {
                        var first = choices[0];

                        // Check finish_reason
                        if (first.TryGetProperty("finish_reason", out var finishElem) &&
                            finishElem.ValueKind == JsonValueKind.String)
                        {
                            yield return new ChatStreamChunk { Content = "", Done = true };
                            yield break;
                        }

                        // Extract delta content
                        if (first.TryGetProperty("delta", out var delta) &&
                            delta.TryGetProperty("content", out var contentElem) &&
                            contentElem.ValueKind == JsonValueKind.String)
                        {
                            var content = contentElem.GetString() ?? "";
                            if (content.Length > 0)
                            {
                                yield return new ChatStreamChunk { Content = content, Done = false };
                            }
                        }
                    }
                }
            }
        }
    }
}
