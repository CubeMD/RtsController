using Interfaces;
using Units.States;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.MovableUnits
{
    public class Engineer : MovableUnit, IReclaim, IBuild 
    {
        public EngineerParameters engineerParameters;

        public void Reclaim(Reclaim reclaim, bool queue)
        {
            AssignState(new ReclaimState(this, reclaim, movingUnitParameters, engineerParameters), queue);
        }

        public void Build(UnitType unitType, Vector3 position, bool queue)
        {
            AssignState(new UnitPlacementState(this, unitType, position, movingUnitParameters, engineerParameters), queue);
        }
        
        public void ConstructUnit(Unit unitUnderConstruction)
        {
            // We are passing false to clear queue to keep the queue and set UnitConstructionState as the active state
            // Done to keep scheduled states as they are and insert construction right after unit placement state
            stateMachine.SetActiveState(new UnitConstructionState(this, unitUnderConstruction, engineerParameters, movingUnitParameters), false);
        }
    }
}