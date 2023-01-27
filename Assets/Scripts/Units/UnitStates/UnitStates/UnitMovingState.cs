using Units.LandUnits;
using Units.UnitStates.UnitStateParameters;
using UnityEngine;
using Utilities.StateMachine;

namespace Units.UnitStates
{
    public class UnitMovingState : UnitState<MovableUnit>
    {
        public MovingUnitParameters movingUnitParameters;
        public Vector3 targetPosition;
        
        public UnitMovingState(Vector3 targetPosition, MovableUnit unit) : base(unit)
        {
            this.movingUnitParameters = unit.movingUnitParameters;
            this.targetPosition = targetPosition;
        }
        
        public override void Step()
        {
            Vector3 relativePosition = targetPosition - unit.transform.position;
            
            if (relativePosition.magnitude <= float.Epsilon)
            {
                Terminate();
                return;
            }
            
            unit.transform.position += relativePosition.normalized * Mathf.Clamp(relativePosition.magnitude, 0, movingUnitParameters.movementSpeed * Time.deltaTime);
        }
    }
}
