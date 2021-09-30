namespace BlazorServerDemo
{
    public static class ActorNames
    {
        public static readonly string ActorSystemName = "Agent";

        public static readonly ActorData AgentActor = new("agent", $"akka://{ActorSystemName}/user");

        public static readonly ActorData SimulationActor = new("simulation", $"akka://{ActorSystemName}/user");
    }

    public class ActorData
    {
        public ActorData(string name, string parent)
        {
            Name = name;
            Path = parent + "/" + name;
        }

        public ActorData(string name)
        {
            Name = name;
            Path = $"akka://{ActorNames.ActorSystemName}/{name}";
        }

        public string Name { get; private set; }

        public string Path { get; private set; }
    }
}
