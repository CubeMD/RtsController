using Systems.Orders;
using Systems.StateMachine.States;
using Templates.Modules;

namespace Systems.Modules
{
    public class ReclaimOrderExecutionModule : OrderExecutionModule
    {
        public float reclaimRange;
        public float reclaimPower;
        
        public ReclaimOrderExecutionModule(ReclaimModuleTemplate reclaimModuleTemplate)
        {
            orderType = reclaimModuleTemplate.orderType;
            reclaimRange = reclaimModuleTemplate.defaultReclaimRange;
            reclaimPower = reclaimModuleTemplate.defaultReclaimPower;
        }
        
        public override ExecutingOrderState GetState(Unit unit, Order order)
        {
            return new ExecutingReclaimOrderState(unit, order, this);
        }
    }
}