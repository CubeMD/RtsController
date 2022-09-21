using Systems.Orders;
using Systems.StateMachine.States;
using Templates.Modules;

namespace Systems.Modules
{
    public class MoveOrderExecutionModule : OrderExecutionModule
    {
        public float movementSpeed;
        public float stoppingDistance;
        
        public MoveOrderExecutionModule(MovementModuleTemplate movementModuleTemplate)
        {
            orderType = movementModuleTemplate.orderType;
            movementSpeed = movementModuleTemplate.defaultMovementSpeed;
            stoppingDistance = movementModuleTemplate.defaultStoppingDistance;
        }
        
        public override ExecutingOrderState GetState(Unit unit, Order order)
        {
            orderType = order.orderType;
            return new ExecutingMoveOrderState(unit, order, this);
        }
    }
}
