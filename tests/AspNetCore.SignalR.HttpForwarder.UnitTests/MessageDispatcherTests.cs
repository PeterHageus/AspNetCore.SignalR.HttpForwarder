using AspNetCore.SignalR.HttpForwarder.Internal;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Xunit;
using MessageHandler = System.Func<AspNetCore.SignalR.HttpForwarder.Internal.SignalRMessage, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace AspNetCore.SignalR.HttpForwarder.UnitTests
{
    public class MessageDispatcherTests
    {
        // Fake dependencies
        private readonly IMessageSenderProvider _messageSenderProvider;
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly IMessageSender _messageSender;

        // SUT
        private readonly MessageDispatcher _dispatcher;

        private MessageHandler _messageHandler;
        private Subject<MessageHook> _hooks;

        public MessageDispatcherTests()
        {
            _hooks = new Subject<MessageHook>(); 
            _messageSenderProvider = A.Fake<IMessageSenderProvider>();
            _logger = A.Fake<ILogger<MessageDispatcher>>();
            _messageSender = A.Fake<IMessageSender>();

            A.CallTo(() => _messageSenderProvider.GetMessageSenderForHub(A<string>._)).Returns(_messageSender);

            _dispatcher = new MessageDispatcher(_messageSenderProvider, _logger, _hooks);
            _messageHandler = _dispatcher.OnMessageReceived;
        }

        [Fact]
        public async Task Messages_Are_Dispatched_To_Recipients()
        {
            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            await _messageHandler(message, CancellationToken.None);

            foreach (var recipient in recipients)
            {
                A.CallTo(() => recipient.SendCoreAsync(_messageSender, message.Method, message.Args, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }
        }  
        
        [Fact]
        public async Task Messages_Are_Dispatched_To_Hooks_Observable()
        {
            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            var tcs = new TaskCompletionSource<MessageHook>();

            using var sub = _hooks
                .Subscribe(x => tcs.TrySetResult(x));

            await _messageHandler(message, CancellationToken.None);

            var actual = await tcs.Task;

            actual.HubTypeName.Should().Be(message.HubTypeName);
            actual.Method.Should().Be(message.Method);
            actual.Args[0].Should().Be(message.Args[0]);
        }

        [Fact]
        public async Task Messages_For_Unknown_Hub_Are_Not_Dispatched()
        {
            A.CallTo(() => _messageSenderProvider.GetMessageSenderForHub(A<string>._)).Returns(null);

            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            await _messageHandler(message, CancellationToken.None);

            foreach (var recipient in recipients)
            {
                A.CallTo(() => recipient.SendCoreAsync(default, default, default, default))
                    .WithAnyArguments()
                    .MustNotHaveHappened();
            }
        }
    }
}
