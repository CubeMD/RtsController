using Targets;
using Units;

namespace Orders
{
    public class ReclaimOrderState : OrderState
    {
        public Reclaim reclaim;
        
        public ReclaimOrderState(Unit assignedUnit, Reclaim reclaim) : base(assignedUnit)
        {
            this.reclaim = reclaim;
        }

        public override void Step()
        {
            unit.Reclaim(this);
        }
    }
}