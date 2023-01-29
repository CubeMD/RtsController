using Systems.StateMachine;
using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class MoveState : State
    {
        protected readonly MovableUnit owner;
        private readonly Vector3 targetPosition;
        private readonly MovingUnitParameters movingUnitParameters;

        protected virtual Vector3 TargetPosition => targetPosition;
        protected virtual bool TerminateOnReach => true;
        protected virtual float StoppingDistance => float.Epsilon;

        public MoveState(MovableUnit owner, Vector3 targetPosition, MovingUnitParameters movingUnitParameters)
        {
            this.owner = owner;
            this.targetPosition = targetPosition;
            this.movingUnitParameters = movingUnitParameters;
        }

        public override void Update()
        {
            base.Update();
            
            if (!TryMoveUnit() && TerminateOnReach)
            {
                TerminateState();
            }
        }
        
        protected bool TryMoveUnit()
        {
            Vector3 relativePosition = TargetPosition - owner.transform.position;
            
            if (relativePosition.magnitude <= StoppingDistance)
            {
                return false;
            }
            
            owner.transform.position += relativePosition.normalized * Mathf.Clamp(relativePosition.magnitude, 0, movingUnitParameters.movementSpeed * Time.deltaTime);
            return true;
        }
        
        public override Vector3 GetStatePosition()
        {
            return TargetPosition;
        }
    }
}