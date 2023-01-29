using Units.Buildings;
using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;
using Utilities;

namespace Units.States
{
    public class UnitPlacementState : MoveState
    {
        private readonly UnitType buildingUnitType;
        private readonly EngineerParameters engineerParameters;

        public UnitPlacementState(MovableUnit owner, UnitType unitType, Vector3 targetPosition, MovingUnitParameters movingUnitParameters, EngineerParameters engineerParameters) : base(owner, targetPosition, movingUnitParameters)
        {
            this.buildingUnitType = unitType;
            this.engineerParameters = engineerParameters;
        }
        
        public override void Update()
        {
            if (!TryMoveUnit())
            {
                Unit buildingUnit = owner.Owner.unitManager.SpawnUnit(buildingUnitType, TargetPosition);
                
                
                if (buildingUnit == null)
                {
                    
                }
                else if (buildingUnit.IsConstructionComplete)
                {
                    TerminateState();
                }
                else
                {
                    buildingUnit.Construct(engineerParameters, owner.Owner.economyManager);
                }
            }
        }
    }
}