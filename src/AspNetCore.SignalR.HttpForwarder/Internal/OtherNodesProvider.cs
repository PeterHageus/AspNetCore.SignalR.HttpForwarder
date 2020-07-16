using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    /// <summary>
    /// Provides an observable with list of other nodes that should br notified of messages
    /// </summary>
    public interface IOtherNodesProvider
    {
        /// <summary>
        /// Observables of other nodes
        /// </summary>
        /// <returns></returns>
        IObservable<Node> Nodes();
    }

    /// <summary>
    /// Represents another node that should be notified of SignalR messages
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The uri of the node
        /// </summary>
        public Uri Uri { get;  }
        
        /// <summary>
        /// Http client used to communicate with node
        /// </summary>
        public Func<HttpClient> Client { get;  }

        /// <summary>
        /// Construct new node
        /// </summary>
        /// <param name="uri">Uri of node</param>
        /// <param name="client">Client</param>
        public Node(Uri uri, Func<HttpClient> client)
        {
            Uri = uri;
            Client = client;
        }
    }
}
