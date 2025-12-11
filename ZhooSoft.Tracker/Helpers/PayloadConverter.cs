using System.Text.Json;

namespace ZhooSoft.Tracker.Helpers
{
    public static class PayloadConverter
    {
        #region Methods

        public static T? ConvertPayload<T>(object? payload)
        {
            try
            {
                if (payload == null)
                    return default;

                // If already the right type
                if (payload is T typed)
                    return typed;

                // Payload coming as JsonElement or string → convert
                var json = payload is JsonElement je
                    ? je.GetRawText()
                    : JsonSerializer.Serialize(payload);

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payload conversion failed: {ex.Message}");
                return default;
            }
        }

        #endregion
    }
}
