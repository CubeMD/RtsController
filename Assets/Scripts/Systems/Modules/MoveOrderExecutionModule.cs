using Systems.Orders;
using Systems.StateMachine.States;
using Templates.Modules;

namespace Systems.Modules
{
    public class MoveOrderExecutionModule : OrderExecutionModule
    {
        public float movementSpeed;

        public MoveOrderExecutionModule(MovementModuleTemplate movementModuleTemplate)
        {
            orderType = OrderType.Move;
            movementSpeed = movementModuleTemplate.defaultMovementSpeed;
        }
        
        public override ExecutingOrderState GetState(Unit unit, Order order)
        {
            return new ExecutingMoveOrderState(unit, order, this);
        }
    }
}
