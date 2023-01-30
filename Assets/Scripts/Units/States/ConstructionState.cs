using Systems.StateMachine;
using Units.States.UnitStateParameters;
using UnityEngine;

namespace Units.States
{
    public class ConstructionState : State
    {
        private readonly Unit owner;
        private readonly UnitStatParameters unitStatParameters;

        public ConstructionState(Unit owner, UnitStatParameters unitStatParameters)
        {
            this.owner = owner;
            this.unitStatParameters = unitStatParameters;
            Debug.Log($"Construction stated {owner.name}");
        }

        public override void Update()
        {
            base.Update();

            if (unitStatParameters.ConstructionPercentage >= 1f)
            {
                unitStatParameters.SetConstructed();
                TerminateState();
                return;
            }
            
            unitStatParameters.AddCurrentMass(unitStatParameters.MassDumped);
            Debug.Log($"Construction of {owner.name}: added {unitStatParameters.MassDumped}, percentage {unitStatParameters.ConstructionPercentage}");
            unitStatParameters.ClearDump();
        }
        
        public override Vector3 GetStatePosition()
        {
            return owner.transform.position;
        }
    }
}