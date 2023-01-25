using System;
using Utilities.StateMachine;

namespace Units.UnitStates
{
    public abstract class UnitState : State
    {
        public Unit unit;

        public UnitState(Unit unit) : base(unit.stateMachine)
        {
            this.unit = unit;
        }

        public override void Terminate()
        {
            stateMachine.currentState = new IdleUnitState(unit);
        }
    }
}