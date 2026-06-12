using System.Text.Json;

namespace SatellitePipeline.Application;

public static class MessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new Dictionary<string, Type>
    {
        [nameof(SatelliteProcessingRequested)] = typeof(SatelliteProcessingRequested),
        [nameof(CreateIndexRequested)] = typeof(CreateIndexRequested),
        [nameof(IndexReady)] = typeof(IndexReady),
        [nameof(CreateTiffRequested)] = typeof(CreateTiffRequested),
        [nameof(TiffReady)] = typeof(TiffReady),
        [nameof(CreatePreviewRequested)] = typeof(CreatePreviewRequested),
        [nameof(PreviewReady)] = typeof(PreviewReady),
        [nameof(SatelliteProcessingCompleted)] = typeof(SatelliteProcessingCompleted)
    };

    public static string GetMessageType<TMessage>() => typeof(TMessage).Name;

    public static string Serialize<TMessage>(TMessage message)
        where TMessage : notnull
    {
        return JsonSerializer.Serialize(message, Options);
    }

    public static object Deserialize(string type, string payloadJson)
    {
        if (!KnownTypes.TryGetValue(type, out var messageType))
            throw new InvalidOperationException($"Unknown message type: {type}");

        return JsonSerializer.Deserialize(payloadJson, messageType, Options)
            ?? throw new InvalidOperationException($"Unable to deserialize message type: {type}");
    }
}
