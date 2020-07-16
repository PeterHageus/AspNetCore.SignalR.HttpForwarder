using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AspNetCore.SignalR.HttpForwarder
{
    /// <summary>
    /// Options used to configure SignalR scale-out with Azure Service Bus.
    /// </summary>
    public class SignalRHttpForwarderOptions
    {
        /// <summary>
        /// The JSON serializer settings.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters =
            {
                new StringEnumConverter()
            }
        };
    }
}
