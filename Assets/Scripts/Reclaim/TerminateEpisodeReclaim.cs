using Unity.MLAgents;

namespace Targets
{
    public class TerminateEpisodeReclaim : Reclaim
    {
        public override bool IsTerminating => true;

        public override void Collect(Agent agent)
        {
            base.Collect(agent);
            agent.EndEpisode();
        }
    }
}
