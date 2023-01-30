using Units.MovableUnits;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class ReclaimState : TargetMoveState
    {
        private readonly Reclaim reclaim;
        private readonly EngineerParameters engineerParameters;

        protected override float StoppingDistance => engineerParameters.Range;

        public ReclaimState(MovableUnit owner, 
            Reclaim reclaim, 
            MovingUnitParameters movingUnitParameters, 
            EngineerParameters engineerParameters) 
            : base(owner, reclaim.transform, movingUnitParameters, false)
        {
            this.reclaim = reclaim;
            this.engineerParameters = engineerParameters;
        }

        public override void Update()
        {
            // Reclaim will be destroyed upon collection
            // So we will need to only check for it and terminate state when needed
            if (reclaim == null)
            {
                TerminateState();
                return;
            }
            
            if (!TryMoveUnit())
            {
                float remainingReclaim = Mathf.Max(reclaim.Amount - engineerParameters.Power * Time.deltaTime, 0);
                float reclaimAmount = reclaim.Amount - remainingReclaim;
                reclaim.Amount = remainingReclaim;
            
                owner.Owner.economyManager.UnitCollectedMass(reclaimAmount);
            }
        }
    }
}