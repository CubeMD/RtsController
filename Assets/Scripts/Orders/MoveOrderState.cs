using Units;
using UnityEngine;

namespace Orders
{
    public class TransformMoveOrderState : MoveOrderState
    {
        private readonly Transform follow;
        public override Vector3 PositionToFollow => follow.position;

        public TransformMoveOrderState(Unit assignedUnit, Transform follow) : base(assignedUnit)
        {
            this.follow = follow;
        }
    }
    
    public class PositionMoveOrderState : MoveOrderState
    {
        public override Vector3 PositionToFollow { get; }

        public PositionMoveOrderState(Unit assignedUnit, Vector3 destination) : base(assignedUnit)
        {
            PositionToFollow = destination;
        }
    }
    
    public class MoveOrderState : OrderState
    {
        public virtual Vector3 PositionToFollow => Vector3.zero;

        protected MoveOrderState(Unit assignedUnit) : base(assignedUnit)
        {
        }

        public override void Step()
        {
            foreach (Unit unit in GetAssignedUnits())
            {
                unit.Move(this);   
            }
        }
    }
}