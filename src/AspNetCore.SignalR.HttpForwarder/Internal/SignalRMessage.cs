using System;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class SignalRMessage
    {
        public string HubTypeName { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SignalRMessageRecipient[] Recipients { get; set; }

        public string Method { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public object[] Args { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(HubTypeName)
                && !string.IsNullOrEmpty(Method)
                && Args != null
                && Recipients != null;
        }
    }
}
