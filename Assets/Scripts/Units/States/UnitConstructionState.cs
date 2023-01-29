using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class UnitConstructionState : MoveState
    {
        private Unit unitUnderConstruction;
        private EngineerParameters engineerParameters;

        public UnitConstructionState(MovableUnit owner, Vector3 targetPosition, MovingUnitParameters movingUnitParameters) : base(owner, targetPosition, movingUnitParameters)
        {
        }
    }
}