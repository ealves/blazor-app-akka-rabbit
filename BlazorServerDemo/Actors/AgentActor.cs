using Akka.Actor;
using Akka.Event;
using Akka.IO;
using Akka.Streams;
using Akka.Streams.Amqp.RabbitMq;
using Akka.Streams.Amqp.RabbitMq.Dsl;
using Akka.Streams.Dsl;
using BlazorServerDemo.Messages;
using System;
using System.Text;

namespace BlazorServerDemo.Actors
{
    #region Messages types
    public class AgentActions
    {
        public AgentActions(AgentActionType actionType)
        {
            ActionType = actionType;
        }

        public AgentActionType ActionType { get; private set; }
    }

    public enum AgentActionType
    {
        Start,
        Stop,
        Restart
    }
    #endregion

    /// <summary>
    /// Address:
    ///     /user/agent
    /// Responsibilities:
    ///     - Creating agent actor
    /// </summary>
    public class AgentActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly AmqpConnectionDetails _connectionSettings;

        private readonly ActorMaterializer _mat;

        public AgentActor()
        {
            Receive<Message>(HandleMessage);
            //Receive<AgentActions>

            _mat = Context.Materializer();

            _connectionSettings = AmqpConnectionDetails.Create("localhost", 5672)
                                                        .WithCredentials(AmqpCredentials.Create("guest", "guest"))
                                                        .WithAutomaticRecoveryEnabled(true)
                                                        .WithNetworkRecoveryInterval(TimeSpan.FromSeconds(1));
        }

        private void HandleMessage(Message message)
        {
            _log.Info($"{GetType().Name} -> HandleMessage -> {message.Content}");

            var exchangeName = "Agent";
            var exchangeDeclaration = ExchangeDeclaration.Create(exchangeName, "topic");

            //queue declaration
            var queueName = "commands";
            var queueDeclaration = QueueDeclaration.Create(queueName)
                                                   .WithDurable(false)
                                                   .WithExclusive(false)
                                                   .WithAutoDelete(false);

            //create sink
            var amqpSink = AmqpSink.CreateSimple(
                AmqpSinkSettings.Create(_connectionSettings)
                                .WithRoutingKey(queueName)
                                .WithDeclarations(queueDeclaration));

            //run sink
            //var input = new[] { "one", "two", "three", "four", "five" };
            //Source.From(input).Select(ByteString.FromString).RunWith(amqpSink, _mat).Wait();

            //create source
            var amqpSource = AmqpSource.AtMostOnceSource(
                NamedQueueSourceSettings.Create(_connectionSettings, queueName).WithDeclarations(queueDeclaration),
                bufferSize: 10);

            amqpSource.Select(m => m.Bytes.ToString(Encoding.UTF8))
                                   .RunForeach(e => _log.Info(e), _mat);

            //amqpSource.Select(m => m.Bytes.ToString(Encoding.UTF8))
            //                       .RunForeach(e => Self.Tell(new AgentActions((AgentActionType)Enum.Parse(typeof(AgentActionType), e))), _mat);

            //result.Wait(TimeSpan.FromSeconds(3));

            //foreach(var item in result.Result)
            //{
            //    _log.Info($"Item {item}");
            //}
        }

        public static Props CreateProps()
        {
            return Props.Create(() => new AgentActor());
        }
    }
}
