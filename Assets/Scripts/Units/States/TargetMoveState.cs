using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class TargetMoveState : MoveState
    {
        private readonly Transform target;

        protected override Vector3 TargetPosition => target.position;
        protected override bool TerminateOnReach { get; }

        public TargetMoveState(MovableUnit owner, 
            Transform target, 
            MovingUnitParameters movingUnitParameters, 
            bool terminateOnReach) : base(owner, target.position, movingUnitParameters)
        {
            this.target = target;
            TerminateOnReach = terminateOnReach;
        }

        public override void Update()
        {
            // Check for target being destroyed
            if (target == null)
            {
                TerminateState();
                return;
            }
            
            base.Update();
        }

        public override Vector3 GetStatePosition()
        {
            return target != null ? TargetPosition : owner.transform.position;
        }
    }
}