using Interfaces;
using Units.States;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.MovableUnits
{
    public abstract class MovableUnit : Unit, IMove
    {
        [SerializeField] protected MovingUnitParameters movingUnitParameters;

        public void Move(Vector3 target, bool queue)
        {
            AssignState(new MoveState(this, target, movingUnitParameters), queue);
        }

        public void Move(Transform target, bool queue)
        {
            AssignState(new TargetMoveState(this, target, movingUnitParameters, false), queue);
        }
    }
}