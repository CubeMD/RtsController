using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.StateMachine
{
    public class StateMachine : MonoBehaviour
    {
        private readonly List<State> stateQueue = new List<State>();

        public State CurrentState => stateQueue.Count > 0 ? stateQueue[0] : null;

        public void Update()
        {
            CurrentState?.Step();
        }
        
        public void AddState(State state, bool toBeginning = false)
        {
            if (state == null)
            {
                Debug.LogError("Can not add empty state");
                return;
            }

            state.OnStateComplete += HandleStateComplete;
            
            if (toBeginning)
            {
                stateQueue.Insert(0, state);
                stateQueue[0].Begin();
                return;
            }

            stateQueue.Add(state);
        }

        public void AddStates(IEnumerable<State> states)
        {
            foreach (State state in states)
            {
                AddState(state);
            }
        }
        
        public bool TryRemoveState(State state, bool stateComplete = false)
        {
            if (state == null)
            {
                Debug.LogError("Can not remove empty state");
                return false;
            }
            
            if (stateQueue.Contains(state))
            {
                if (stateComplete)
                {
                    state.Complete();
                }
                stateQueue.Remove(state);
                return true;
            }

            return false;
        }

        public void ClearStateQueue(bool completeCurrent = false)
        {
            if (completeCurrent)
            {
                CurrentState.Complete();
            }
            
            stateQueue.Clear();
        }

        private void HandleStateComplete(State state)
        {
            state.OnStateComplete -= HandleStateComplete;
            TryRemoveState(state);
        }
    }
}
