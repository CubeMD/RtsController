using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class UnitConstructionState : TargetMoveState
    {
        private readonly Unit unitUnderConstruction;
        private readonly EngineerParameters engineerParameters;

        protected override float StoppingDistance => engineerParameters.Range;

        public UnitConstructionState(MovableUnit owner, Unit unitUnderConstruction, EngineerParameters engineerParameters, MovingUnitParameters movingUnitParameters) 
            : base(owner, unitUnderConstruction.transform, movingUnitParameters, false)
        {
            this.unitUnderConstruction = unitUnderConstruction;
            this.engineerParameters = engineerParameters;
        }

        public override void Update()
        {
            // Check for target being destroyed
            if (unitUnderConstruction == null || unitUnderConstruction.IsConstructed)
            {
                TerminateState();
                return;
            }
            
            if (!TryMoveUnit())
            {
                float amount = engineerParameters.Power * Time.deltaTime;
                unitUnderConstruction.DumpMass(amount);
            }
        }
    }
}