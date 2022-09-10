using Unity.MLAgents;

namespace Targets
{
    public class TerminateTarget : Target
    {
        public override void Collect(Agent agent)
        {
            base.Collect(agent);
            agent.EndEpisode();
        }
    }
}
