namespace Systems.StateMachine
{
    public abstract class State
    {
        public event System.Action<State> OnSelfTerminate;
    
        /// <summary>
        /// Called by the StateMachine upon its step
        /// </summary>
        public virtual void Step() { }

        /// <summary>
        /// Called by the StateMachine when the State begins
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Called by the StateMachine when this State terminates
        /// </summary>
        public virtual void Terminate() { }
    
        /// <summary>
        /// Should be called to signal self completion
        /// </summary>
        public void SelfTerminate()
        {
            Terminate();
            OnSelfTerminate?.Invoke(this);
        }
    }
}
