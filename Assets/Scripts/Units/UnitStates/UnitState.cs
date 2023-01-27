using System;
using Utilities.StateMachine;

namespace Units.UnitStates
{
    public abstract class UnitState<T> : State where T : Unit
    {
        public T unit;

        public UnitState(T unit) : base(unit.stateMachine)
        {
            this.unit = unit;
        }
    }
}