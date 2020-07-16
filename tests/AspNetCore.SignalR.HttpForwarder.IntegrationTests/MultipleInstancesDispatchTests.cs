using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.HttpForwarder.Internal;
using AspNetCore.SignalR.HttpForwarder.TestApp;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AspNetCore.SignalR.HttpForwarder.IntegrationTests
{
    public class MultipleInstancesDispatchTests
    {
        private readonly ITestOutputHelper _output;

        public MultipleInstancesDispatchTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Simple_scenario()
        {
            var sourceProxys = new ListNodeProvider();
            var source = CreateServer("source", sourceProxys);
            var target = CreateServer("target", new ListNodeProvider());
            var sourceClient = CreateClient(source);
            var targetClient = CreateClient(target);

            sourceProxys.Add(new Node(target.BaseAddress, () => target.CreateClient()));

            var targetReceived = new SemaphoreSlim(0, 1);
            var targetReceivedMessage = String.Empty;

            targetClient.On<string>("ReceiveMessage", message =>
            {
                targetReceivedMessage = message;
                targetReceived.Release();
            });

            await sourceClient.StartAsync();
            await targetClient.StartAsync();

            var actual = "This is the message";

            await sourceClient.SendAsync("SendMessageAsync", actual);

            await targetReceived.WaitAsync(5000);

            targetReceivedMessage.Should().Be(actual);
        }

        [Fact]
        public async Task Clients_Receive_Messages_Sent_From_Other_Server_Instance()
        {
            var node1 = new ListNodeProvider();
            var node2 = new ListNodeProvider();

            TestServer
                server1 = CreateServer("server1", node2),
                server2 = CreateServer("server2", node1);

            HubConnection
                client1 = CreateClient(server1),
                client2 = CreateClient(server2);

            node1.Add(new Node(server1.BaseAddress, () => server1.CreateClient()));
            node2.Add(new Node(server2.BaseAddress, () => server2.CreateClient()));

            // Use semaphore instead of manual reset event because
            // it supports async
            SemaphoreSlim
                client1ReceivedMessage = new SemaphoreSlim(0, 1),
                client2ReceivedMessage = new SemaphoreSlim(0, 1);

            string
                messageReceivedByClient1 = null,
                messageReceivedByClient2 = null;

            client1.On<string>("ReceiveMessage", message =>
            {
                messageReceivedByClient1 = message;
                client1ReceivedMessage.Release();
            });

            client2.On<string>("ReceiveMessage", message =>
            {
                messageReceivedByClient2 = message;
                client2ReceivedMessage.Release();
            });

            await client1.StartAsync();
            await client2.StartAsync();

            string
                messageFromClient1 = "Hello world from 1",
                messageFromClient2 = "Hello world from 2";

            await client1.SendAsync("SendMessageAsync", messageFromClient1);
            await client2.SendAsync("SendMessageAsync", messageFromClient2);

            await Task.WhenAll(
                client1ReceivedMessage.WaitAsync(5000),
                client2ReceivedMessage.WaitAsync(5000));

            messageReceivedByClient1.Should().Be(messageFromClient2);
            messageReceivedByClient2.Should().Be(messageFromClient1);
        }

        private TestServer CreateServer(string baseAdress, ListNodeProvider other)
        {
            var uri = new Uri($"http://{baseAdress}");
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration(config => config.AddUserSecrets<Startup>().AddEnvironmentVariables())
                .ConfigureServices(s =>
                {
                    s.AddSingleton<SignalRHttpForwarderOptions>();
                    s.AddTransient<IOtherNodesProvider>(_ => other);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddXUnit(_output);
                })
                .UseUrls(uri.ToString());

            return new TestServer(builder) {BaseAddress = uri};
        }

        private class ListNodeProvider : IOtherNodesProvider
        {
            private readonly Subject<Node> _other = new Subject<Node>();
            public IObservable<Node> Nodes() => _other;
            public void Add(Node node) => _other.OnNext(node);
        }

        private HubConnection CreateClient(TestServer server)
        {
            return new HubConnectionBuilder()
                .WithUrl(
                    new Uri(server.BaseAddress, "/hub/chat"),
                    options =>
                    {
                        // TestServer is in-memory, we can't really connect
                        // via HTTP. Use LongPolling and TestServer's handler
                        // to send requests instead.
                        options.Transports = HttpTransportType.LongPolling;
                        options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddXUnit();
                })
                .Build();
        }
    }
}