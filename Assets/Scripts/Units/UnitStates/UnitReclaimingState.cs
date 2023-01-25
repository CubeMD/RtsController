using UnityEngine;

namespace Units.UnitStates
{
    public class UnitReclaimingState : UnitState
    {
        private float reclaimRange;
        private float reclaimPower;
        private Reclaim reclaim;

        public UnitReclaimingState(float reclaimRange, float reclaimPower, Reclaim reclaim, Unit unit) : base(unit)
        {
            this.reclaimRange = reclaimRange;
            this.reclaimPower = reclaimPower;
            this.reclaim = reclaim;
        }

        public override void Step()
        {
            Vector3 reclaimOffset = reclaim.transform.position - unit.transform.position;

            if (!(reclaimOffset.magnitude <= reclaimRange))
            {
                Terminate();
                return;
            }
            
            float remainingReclaim = Mathf.Max(reclaim.Amount - reclaimPower * Time.deltaTime, 0);
            float reclaimAmount = reclaim.Amount - remainingReclaim;
            reclaim.Amount = remainingReclaim;
            
            unit.owner.economyManager.UnitCollectedMass(reclaimAmount);
        }
    }
}