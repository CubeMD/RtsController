using System;
using Systems.Orders;

namespace Systems.Modules
{
    public abstract class OrderExecutionModule : Module
    {
        public event Action<Unit, Order> OnOrderExecutionModuleCompletedOrder; 
        
        public OrderType orderType;
        public Order executedOrder;
        
        public OrderExecutionModule(Unit unit) : base(unit) { }

        public virtual void SetExecutedOrder(Order order)
        {
            executedOrder = order;
        }

        public virtual void ClearActiveOrder()
        {
            executedOrder = null;
        }
        
        protected void OrderCompleted()
        {
            OnOrderExecutionModuleCompletedOrder?.Invoke(unit, executedOrder);
        }
    }
}