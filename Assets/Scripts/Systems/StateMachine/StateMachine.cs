using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Systems.StateMachine
{
    /// <summary>
    /// GameObject Component that manages states
    /// States can be queued to be executed in order
    /// </summary>
    public class StateMachine: MonoBehaviour
    {
        /// <summary>
        /// Called when a state resolves, either from an early termination or finishing its process
        /// Is not called when an active state is terminated by replacing it with a new state
        /// </summary>
        public event System.Action<State> OnActiveStateTerminatedEarly;
        public event System.Action<StateMachine> OnStateMachineUpdate;
        public event System.Action<StateMachine> OnStateMachineFixedUpdate;
        public event System.Action<StateMachine> OnStateMachineLateUpdate;

        /// <summary>
        /// Called when a new state is set active
        /// </summary>
        public event System.Action<State> OnNewStateBegun;

        [Header("Debug")]
        [SerializeField] private bool renderAllStatesVisuals;
        [SerializeField] private LineRenderer lineRenderer;

        public int QueueAmount => stateQueue.Count;
        public State LastState => lastAddedState;

        private State activeState;
        private State lastAddedState;
        private List<State> stateQueue = new List<State>();
        private bool isDebuggingEnabled;

#if UNITY_EDITOR
        private bool exitingPlayMode;
#endif

        protected virtual void Awake()
        {
            if (activeState == null)
            {
                SetActiveState(new EmptyState());
            }

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EDITOR_PlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void EDITOR_PlayModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                exitingPlayMode = true;
            }
        }
#endif

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EDITOR_PlayModeStateChanged;

            if (!exitingPlayMode)
            {
                TerminateAllStates();
            }
#else
            TerminateAllStates();
#endif
        }

        public void TerminateAllStates()
        {
            SetActiveState(new EmptyState());
        }

        protected virtual void Update()
        {
            activeState?.Update();
            RenderStateLines();
            OnStateMachineUpdate?.Invoke(this);
        }

        protected virtual void FixedUpdate()
        {
            activeState?.FixedUpdate();
            OnStateMachineFixedUpdate?.Invoke(this);
        }

        protected virtual void LateUpdate()
        {
            activeState?.LateUpdate();
            BroadcastLateUpdate();
        }

        protected void BroadcastLateUpdate()
        {
            OnStateMachineLateUpdate?.Invoke(this);
        }

        /// <summary>
        /// Enables debugging on this StateMachine.  When enabled, logs are output to the console for State changes
        /// </summary>
        /// <param name="debuggingEnabled">If true, enables debugging; otherwise disable debugging</param>
        public void EnableDebugging(bool debuggingEnabled)
        {
            isDebuggingEnabled = debuggingEnabled;
        }

        /// <summary>
        /// Gets the currently active State for this StateMachine
        /// </summary>
        /// <returns></returns>
        public State GetActiveState()
        {
            return activeState;
        }

        /// <summary>
        /// Terminates the active State, sets the given State to active immediately and clears the State queue.
        /// </summary>
        /// <param name="state">State to set active</param>
        /// <param name="clearQueue">If true, will clear the state and assign this state as last added</param>
        public virtual void SetActiveState(State state, bool clearQueue = true)
        {
            if (clearQueue)
            {
                stateQueue.Clear();
                lastAddedState = state;
            }
            
            TransitionState(state, true);
        }

        /// <summary>
        /// Terminates the current active State and makes the given State active
        /// </summary>
        /// <param name="state">State to transition to</param>
        /// <param name="terminateActiveState">If true, calls OnTerminateState on the active State before the transition</param>
        private void TransitionState(State state, bool terminateActiveState)
        {
            if (activeState != null)
            {
                if (terminateActiveState)
                {
                    activeState.OnTerminateState();
                }

                activeState.OnSelfTerminate -= OnActiveStateSelfTerminate;
                activeState = null;
            }

            if (state != null)
            {
                activeState = state;
                activeState.OnSelfTerminate += OnActiveStateSelfTerminate;
                activeState.OnBeginState();

                OnNewStateBegun?.Invoke(state);
            }

            if (isDebuggingEnabled)
            {
                Debug.Log(string.Format("{0} begun new state: {1}", gameObject.name, state.GetType()));
            }
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

            if (activeState is EmptyState)
            {
                if (isDebuggingEnabled)
                {
                    Debug.Log("Attempting to queue after empty state.  Overriding active state");
                }

                SetActiveState(state);
                return;
            }

            lastAddedState = state;
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

        private void OnActiveStateSelfTerminate(State terminatedState)
        {
            if (stateQueue.Count <= 0)
            {
                TransitionState(new EmptyState(), false);
            }
            else
            {
                State firstQueuedState = stateQueue[0];
                stateQueue.RemoveAt(0);
                TransitionState(firstQueuedState, false);
            }

            if (OnActiveStateTerminatedEarly != null)
            {
                OnActiveStateTerminatedEarly(terminatedState);
            }
        }

        private void OnDrawGizmos()
        {
            activeState?.OnDrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            activeState?.OnDrawGizmosSelected();
        }
        
        private void RenderStateLines()
        {
            List<Vector3> points = new List<Vector3>{transform.position + Vector3.up};

            if (activeState != null && !(activeState is EmptyState))
            {
                addStatePointsToList(activeState);
            }
            
            if (renderAllStatesVisuals)
            {
                foreach (State state in stateQueue)
                {
                    addStatePointsToList(state);
                }
            }

            void addStatePointsToList(State state)
            {
                points.Add(state.GetStatePosition() + Vector3.up);
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}