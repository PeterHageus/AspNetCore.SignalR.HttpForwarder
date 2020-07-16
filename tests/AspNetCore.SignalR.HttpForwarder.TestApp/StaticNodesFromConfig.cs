using System;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AspNetCore.SignalR.HttpForwarder.Internal;
using Microsoft.Extensions.Configuration;

namespace AspNetCore.SignalR.HttpForwarder.TestApp
{
    public class StaticNodesFromConfig : IOtherNodesProvider
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;

        public StaticNodesFromConfig(IHttpClientFactory factory, IConfiguration configuration)
        {
            _factory = factory;
            _configuration = configuration;
        }

        public IObservable<Node> Nodes() => Observable.Create<Node>(observer =>
        {
            foreach(var node in _configuration["Nodes"].Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries))
                observer.OnNext(new Node(new Uri(node.Trim()), () => _factory.CreateClient("Forwarder")));
            
            return Disposable.Empty;
        });
    }
}