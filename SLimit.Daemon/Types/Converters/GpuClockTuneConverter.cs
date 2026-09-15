using GpuSSharp.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SLimit.Daemon.Types.Converters;

public sealed class GpuClockTuneConverter : JsonConverter<GpuClockTune>
{
    public override bool CanWrite => false;

    public override GpuClockTune? ReadJson(
        JsonReader reader,
        Type objectType,
        GpuClockTune? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject obj = JObject.Load(reader);
        
        T Read<T>(string name)
        {
            JToken? token = obj[name];

            if (token is null || token.Type == JTokenType.Null)
                throw new JsonSerializationException(
                    $"Missing field: {name}");

            return token.ToObject<T>(serializer)!;
        }

        if (obj["OffsetMhz"] is not null)
            return new GpuClockTune.Offset(
                Read<int>("OffsetMhz"),
                Read<GpuPState>("PState"));

        if (obj["Percent"] is not null)
            return new GpuClockTune.Overdrive(
                Read<uint>("Percent"));

        if (obj["MinMhz"] is not null || obj["MaxMhz"] is not null)
            return new GpuClockTune.ClockRange(
                Read<uint>("MinMhz"),
                Read<uint>("MaxMhz"));

        throw new JsonSerializationException(
            $"Unknown GpuClockTune format: {obj}");
    }

    public override void WriteJson(
        JsonWriter writer,
        GpuClockTune? value,
        JsonSerializer serializer)
        => throw new NotSupportedException();
}