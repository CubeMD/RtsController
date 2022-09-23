using Systems.Orders;

namespace Systems.StateMachine.States
{
    public abstract class ExecutingOrderState : State
    {
        public Unit unit;
        public Order order;

        public ExecutingOrderState(Unit unit, Order order)
        {
            this.unit = unit;
            this.order = order;
        }
        
        public override void Complete()
        {
            base.Complete();
            if (order != null)
            {
                order.TryRemoveUnit(unit);
            }
        }
    }
}