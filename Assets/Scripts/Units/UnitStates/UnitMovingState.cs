using UnityEngine;
using Utilities.StateMachine;

namespace Units.UnitStates
{
    public class UnitMovingState : UnitState
    {
        private Vector3 targetPosition;
        private float movementSpeed;

        public UnitMovingState(Vector3 targetPosition, float movementSpeed, Unit unit) : base(unit)
        {
            this.targetPosition = targetPosition;
            this.movementSpeed = movementSpeed;
        }
        
        public override void Step()
        {
            Vector3 relativePosition = targetPosition - unit.transform.position;

            if (relativePosition.magnitude <= float.Epsilon)
            {
                Terminate();
                return;
            }
            
            unit.transform.position += relativePosition.normalized * Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);
        }
    }
}
