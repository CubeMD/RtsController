using System.Collections.Generic;

namespace Utilities.StateMachine
{
    public class StateMachine
    {
        public readonly List<State> queuedStates = new List<State>();

        public void Step()
        {
            if (queuedStates.Count > 0)
            {
                queuedStates[0].Step();
            }
        }
    }
}