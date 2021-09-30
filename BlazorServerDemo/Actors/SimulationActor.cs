using Akka.Actor;
using Akka.Event;
using BlazorServerDemo.Messages;
using System;

namespace BlazorServerDemo.Actors
{
    #region Messages
    public class StartSimulation
    {

    }
    #endregion

    /// <summary>
    /// Address:
    ///     /user/simulation
    /// Responsibilities:
    ///     - Creating simulation actor
    /// </summary>
    public class SimulationActor : ReceiveActor
    {
        private static IActorRef _agentActor;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public SimulationActor(IActorRef actorRef)
        {
            _agentActor = actorRef;

            Receive<StartSimulation>(HandleStartSimulation);
            Receive<Message>(HandleMessage);
        }

        private void HandleStartSimulation(StartSimulation startSimulation)
        {
            _log.Info($"{GetType().Name} -> HandleStartSimulation");

            _agentActor.Tell(new Message("teste"));
        }

        private void HandleMessage(Message message)
        {
            _log.Info($"{GetType().Name} -> HandleMessage -> {message.Content}");
        }

        public static Props CreateProps(IActorRef actorRef)
        {
            return Props.Create(() => new SimulationActor(actorRef));
        }
    }
}
