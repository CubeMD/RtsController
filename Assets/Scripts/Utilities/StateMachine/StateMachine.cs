namespace Utilities.StateMachine
{
    public class StateMachine
    {
        public State currentState;

        public void Step()
        {
            currentState?.Step();
        }
    }
}