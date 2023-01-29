using UnityEngine;

namespace Systems.StateMachine
{
    public class EmptyState : State
    {
        public override Vector3 GetStatePosition()
        {
            return Vector3.zero;
        }
    }
}