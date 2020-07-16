using System;
using System.IO;
using AspNetCore.SignalR.HttpForwarder;
using AspNetCore.SignalR.HttpForwarder.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring Azure Service Bus-based scale-out for a SignalR Server in an <see cref="ISignalRServerBuilder" />.
    /// </summary>
    public static class SignalRHttpForwarderServiceCollectionExtensions
    {
        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using an Azure Service Bus topic.
        /// </summary>
        /// <param name="builder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="configureOptions">A callback to configure the service bus options.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static ISignalRServerBuilder AddHttpForwarder(this ISignalRServerBuilder builder, Action<SignalRHttpForwarderOptions> configureOptions = null)
        {
            if (configureOptions == null)
                builder.Services.AddSingleton(new SignalRHttpForwarderOptions());
            else
                builder.Services.Configure(configureOptions);

            builder.Services.AddSingleton<IMessageSenderProvider, MessageSenderProvider>();
            builder.Services.AddSingleton(typeof(MessageSenderHubLifetimeManager<>));
            builder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(HttpForwarderHubLifetimeManager<>));
            builder.Services.AddSingleton<MessageDispatcher>();
            builder.Services.AddSingleton<MessageForwarder>();
            builder.Services.AddHostedService<MessageForwarderBootstrapper>();
            builder.Services.AddSingleton<IForwarder>(s => s.GetRequiredService<MessageForwarder>());

            return builder;
        }

        /// <summary>
        /// Map an endpoint for handling propagated SignalR messages
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapHttpForwarder(this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<SignalRHttpForwarderOptions>();

            endpoints.MapPost(Endpoint, async context =>
            {
                var dispatcher = context.RequestServices.GetRequiredService<MessageDispatcher>();
                using var reader = new StreamReader(context.Request.Body);
                var json = await reader.ReadToEndAsync();
                var message = JsonConvert.DeserializeObject<SignalRMessage>(json, options.SerializerSettings);

                await dispatcher.OnMessageReceived(message, context.RequestAborted);

                context.Response.StatusCode = 200;
                await context.Response.CompleteAsync();
            });

            return endpoints;
        }

        internal const string Endpoint = "/signalrhttpforwarder";
    }
}