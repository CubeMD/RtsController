using System.Collections.Generic;
using Systems.StateMachine;
using Units;

namespace Orders
{
    public abstract class OrderState : State
    {
        private readonly List<Unit> assignedUnits;

        protected OrderState(Unit assignedUnit)
        {
            assignedUnits = new List<Unit> {assignedUnit};
        }
        
        protected OrderState(List<Unit> assignedUnits)
        {
            this.assignedUnits = assignedUnits;
        }

        public List<Unit> GetAssignedUnits()
        {
            return assignedUnits;
        }

        public void AddAssignedUnit(Unit unit)
        {
            assignedUnits.Add(unit);
        }

        public void AddAssignedUnit(IEnumerable<Unit> units)
        {
            assignedUnits.AddRange(units);
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);
                return true;
            }
            
            return false;
        }
    }
}