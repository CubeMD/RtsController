namespace Utilities.StateMachine
{
    public class State
    {
        protected StateMachine stateMachine;

        public State(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
        
        public virtual void Step()
        {
            
        }

        public virtual void Terminate()
        {
            
        }
    }
}