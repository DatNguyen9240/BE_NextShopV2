using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NextShopV2.Shared.Json
{
    public class DateTimeUtcConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return default;
            if (DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt))
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToUniversalTime();
            }
            return DateTime.SpecifyKind(DateTime.Parse(s), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToUniversalTime();
            writer.WriteStringValue(utc.ToString("o"));
        }
    }
}
