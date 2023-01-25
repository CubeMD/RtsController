using System.Collections.Generic;
using Units;

namespace Agents
{
    public class RtsAgentAction
    {
        public List<Unit> selectedUnits;
            
        public RtsAgentAction(List<Unit> selectedUnits)
        {
            this.selectedUnits = selectedUnits;
        }

        public RtsAgentAction()
        {
            
        }
    }

    public class RtsAgentObservation
    {
        public float[] vectorObservations;
        public List<List<float[]>> observationBuffers;

        public RtsAgentObservation(float[] vectorObservations, List<List<float[]>> observationBuffers)
        {
            this.vectorObservations = vectorObservations;
            this.observationBuffers = observationBuffers;
        }
    }


    public class AgentStep
    {
        public RtsAgentAction rtsAgentAction;
        public RtsAgentObservation rtsAgentObservation;
    }
}