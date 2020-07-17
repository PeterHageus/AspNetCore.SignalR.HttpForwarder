namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    /// <summary>
    /// Allows the hosting node to react to messages forwarded
    /// </summary>
    public class MessageHook
    {
        /// <summary>
        /// Type name of hub
        /// </summary>
        public string HubTypeName { get;  }
        
        /// <summary>
        /// Method name
        /// </summary>
        public string Method { get;  }
        
        /// <summary>
        /// Arguments
        /// </summary>
        public object[] Args { get;  }

        internal MessageHook(string hubTypeName, string method, object[] args)
        {
            HubTypeName = hubTypeName;
            Method = method;
            Args = args;
        }
    }
}