using Akka.Actor;
using Akka.Configuration;
using Akka.DependencyInjection;
using BlazorServerDemo.Actors;
using BlazorServerDemo.Messages;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorServerDemo.Services
{
    public class AkkaService : IPublishService, IHostedService
    {
        private ActorSystem _actorSystem;

        public IActorRef AgentActor { get; private set; }

        public IActorRef SimulationActor { get; private set; }


        private readonly IServiceProvider _sp;

        public AkkaService(IServiceProvider sp)
        {
            _sp = sp;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var hocon = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("akka.conf", cancellationToken));
            var bootstrap = BootstrapSetup.Create().WithConfig(hocon);
            var di = DependencyResolverSetup.Create(_sp);
            var actorSystemSetup = bootstrap.And(di);
            _actorSystem = ActorSystem.Create(ActorNames.ActorSystemName, actorSystemSetup);

            var agentManagerProps = DependencyResolver.For(_actorSystem).Props<AgentActor>();
            AgentActor = _actorSystem.ActorOf(agentManagerProps, ActorNames.AgentActor.Name);

            var simulationProps = DependencyResolver.For(_actorSystem).Props<SimulationActor>(AgentActor);
            SimulationActor = _actorSystem.ActorOf(simulationProps, ActorNames.SimulationActor.Name);

            SimulationActor.Tell(new StartSimulation());

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }

        public async Task<Message> SendMessage(string input, CancellationToken token)
        {
            return await AgentActor.Ask<Message>(input, token);
        }
    }
}
