using Units.LandUnits;
using Units.UnitStates.UnitStateParameters;
using UnityEngine;
using Utilities.StateMachine;

namespace Units.UnitStates
{
    public class UnitReclaimingState : UnitState<MovableUnit>
    {
        private ReclaimingUnitParameters reclaimingUnitParameters;
        private Reclaim reclaim;
        private readonly StateMachine reclaimingStateMachine = new StateMachine();

        public UnitReclaimingState(ReclaimingUnitParameters reclaimingUnitParameters, Reclaim reclaim, MovableUnit unit) : base(unit)
        {
            this.reclaimingUnitParameters = reclaimingUnitParameters;
            this.reclaim = reclaim;
        }

        public override void Step()
        {
            Vector3 reclaimOffset = reclaim.transform.position - unit.transform.position;

            if (!(reclaimOffset.magnitude <= reclaimingUnitParameters.reclaimRange))
            {
                if (reclaimingStateMachine.queuedStates.Count < 1)
                {
                    reclaimingStateMachine.queuedStates.Add(new UnitMovingState(reclaim.transform.position, unit)); 
                }
                else if (!(reclaimingStateMachine.queuedStates[0] is UnitMovingState))
                {
                    reclaimingStateMachine.queuedStates.Clear();
                    reclaimingStateMachine.queuedStates.Add(new UnitMovingState(reclaim.transform.position, unit));
                }
                
                reclaimingStateMachine.queuedStates[0].Step();
                
                return;
            }
            
            float remainingReclaim = Mathf.Max(reclaim.Amount - reclaimingUnitParameters.reclaimPower * Time.deltaTime, 0);
            float reclaimAmount = reclaim.Amount - remainingReclaim;
            reclaim.Amount = remainingReclaim;
            
            unit.owner.economyManager.UnitCollectedMass(reclaimAmount);
        }
    }
}