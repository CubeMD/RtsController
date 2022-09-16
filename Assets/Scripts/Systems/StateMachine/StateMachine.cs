using System.Collections.Generic;

namespace Systems.StateMachine
{
    public class StateMachine
    {
        public event System.Action<State> OnActiveStateTerminated;
        public event System.Action<StateMachine> OnStateMachineUpdate;
        public event System.Action<State> OnNewStateBegun;

        private State activeState;
        private readonly List<State> stateQueue = new List<State>();

        public StateMachine(State state)
        {
            if (state != null)
            {
                SetActiveState(state);
            }
        }
        
        public StateMachine()
        {
            
        }

        public void Step()
        {
            activeState?.Step();

            OnStateMachineUpdate?.Invoke(this);
        }

        /// <summary>
        /// Gets the currently active State for this StateMachine
        /// </summary>
        public State GetActiveState()
        {
            return activeState;
        }

        /// <summary>
        /// Terminates the active State, sets the given State to active immediately and clears the State queue
        /// </summary>
        /// <param name="state">State to set active</param>
        /// <param name="terminateActive">If true, calls OnTerminateState on the active State before the transition</param>
        public void SetActiveState(State state, bool terminateActive = true)
        {
            stateQueue.Clear();
            TransitionState(state, terminateActive);
        }

        public void ClearStatesAndTerminate()
        {
            stateQueue.Clear();
            SetActiveState(null);
        }
        
        /// <summary>
        /// Queues a State.  Queued States will become active in order as previous states terminate.
        /// </summary>
        /// <param name="state">State to queue</param>
        public void QueueState(State state)
        {
            if (state == null)
            {
                return;
            }

            if (activeState == null)
            {
                SetActiveState(state);
                return;
            }

            stateQueue.Add(state);
        }

        /// <summary>
        /// Queues several States at once
        /// </summary>
        /// <param name="states">States to queue, in order</param>
        public void QueueState(IEnumerable<State> states)
        {
            foreach (State state in states)
            {
                QueueState(state);
            }
        }

        /// <summary>
        /// Transitions current active State to a new one
        /// </summary>
        /// <param name="state">State to transition to</param>
        /// <param name="terminate">If true, calls OnTerminateState on the active State before the transition</param>
        private void TransitionState(State state, bool terminate)
        {
            if (activeState != null)
            {
                if (terminate)
                {
                    activeState.Terminate();
                }

                activeState.OnSelfTerminate -= OnActiveStateSelfTerminate;
                activeState = null;
            }

            if (state != null)
            {
                activeState = state;
                activeState.OnSelfTerminate += OnActiveStateSelfTerminate;
                activeState.Begin();

                if (OnNewStateBegun != null)
                {
                    OnNewStateBegun(state);
                }
            }
        }
        
        private void OnActiveStateSelfTerminate(State terminatedState)
        {
            if (stateQueue.Count == 0)
            {
                TransitionState(null, false);
            }
            else
            {
                State firstQueuedState = stateQueue[0];
                stateQueue.RemoveAt(0);
                TransitionState(firstQueuedState, false);
            }

            if (OnActiveStateTerminated != null)
            {
                OnActiveStateTerminated(terminatedState);
            }
        }
    }
}
