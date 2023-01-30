using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class UnitPlacementState : MoveState
    {
        private readonly Engineer ownerEngineer;
        private readonly EngineerParameters engineerParameters;
        private readonly UnitType buildingUnitType;
        private readonly UnitManager manager;
        private Unit ghostUnit;

        protected override float StoppingDistance => engineerParameters.Range;

        public UnitPlacementState(Engineer owner, UnitType unitType, Vector3 targetPosition, MovingUnitParameters movingUnitParameters, EngineerParameters engineerParameters) : base(owner, targetPosition, movingUnitParameters)
        {
            ownerEngineer = owner;
            this.engineerParameters = engineerParameters;
            buildingUnitType = unitType;
            manager = ownerEngineer.Owner.unitManager;
        }

        public override void OnBeginState()
        {
            base.OnBeginState();
            
            if (!manager.CanBuildUnitTypeAtPosition(buildingUnitType, TargetPosition))
            {
                TerminateState();
            }

            ghostUnit = manager.PlaceUnitGhost(buildingUnitType, TargetPosition);
        }

        public override void Update()
        {
            if (ghostUnit == null)
            {
                TerminateState();
            }
            
            if (!TryMoveUnit())
            {
                manager.EnableUnit(ghostUnit);
                ownerEngineer.ConstructUnit(ghostUnit);

                TerminateState();
            }
        }
    }
}