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
    }
}