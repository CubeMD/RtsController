using Systems.Orders;
using Systems.StateMachine.States;

namespace Systems.Modules
{
    public abstract class OrderExecutionModule : Module
    {
        public OrderType orderType;
        public abstract ExecutingOrderState GetState(Unit unit, Order order);
    }
}