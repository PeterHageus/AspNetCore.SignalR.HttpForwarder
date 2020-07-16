# AspNetCore.SignalR.HttpForwarder

Provides scale-out support for ASP.NET Core SignalR using an HTTP forwarder to dispatch messages to all server instances.

Based on [AspNetCore.SignalR.AzureServiceBus](https://github.com/thomaslevesque/AspNetCore.SignalR.AzureServiceBus) by Thomas Levesque

## Status 

This is currently experimental. Only tested with Asp.Net Core 3.1.

(build is pending)

## How to use it

Install the `AspNetCore.SignalR.HttpForwarder` package, and add this to your `Startup.ConfigureServices` method:

```csharp
services.AddSignalR()
        .AddHttpForwarder();
```

You must implement and register the interface `IOtherNodesProvider` for discovery of other nodes. They are exposed as an observable to enable new nodes to be added dynamically.

Example reading from config:
```csharp
  public IObservable<Node> Nodes() => Observable.Create<Node>(observer =>
        {
            foreach(var node in _configuration["Nodes"].Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries))
                observer.OnNext(new Node(new Uri(node.Trim()), () => _factory.CreateClient("Forwarder")));
            
            return Disposable.Empty;
        });
```

```csharp
 services.AddTransient<IOtherNodesProvider, StaticNodesFromConfig>();
```

Then map an endpoint to receive messages from other nodes om `Startup.Configure`

```csharp
app.UseEndpoints(endpoints =>
            {
                endpoints.MapHttpForwarder();
                ...
            });
```

The TestApp has an example using Polly and HttpClientFactory for resiliance.
