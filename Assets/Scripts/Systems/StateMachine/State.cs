using System;

namespace Systems.StateMachine
{
    public abstract class State
    {
        public event Action<State> OnStateComplete; 
        
        /// <summary>
        /// Called by the StateMachine upon its step
        /// </summary>
        public virtual void Step() { }

        /// <summary>
        /// Called by the StateMachine when the State begins
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Called by the StateMachine when this State completes
        /// </summary>
        public virtual void Complete()
        {
            OnStateComplete?.Invoke(this);
        }
    }
}
