using Systems.Orders;

namespace Systems.Modules
{
    public class OrderExecutionModule : Module
    {
        public OrderType orderType;
        
        public OrderExecutionModule(Unit unit) : base(unit) { }

        public virtual void SetOrder(Order order)
        {
            active = true;
        }

        public virtual void UnSetOrder()
        {
            active = false;
        }

        public virtual void Complete()
        {
            unit.HandleUnitCompletedOrder();
        }
    }
}