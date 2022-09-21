using Systems.Modules;
using Systems.Orders;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class ExecutingMoveOrderState : ExecutingOrderState
    {
        private Order.MoveData orderData;
        private MoveOrderExecutionModule moveOrderExecutionModule;
        
        public ExecutingMoveOrderState(Unit unit, Order order, MoveOrderExecutionModule moveOrderExecutionModule)
            : base(unit, order)
        {
            this.moveOrderExecutionModule = moveOrderExecutionModule;
            orderData = order.orderData as Order.MoveData;
        }
        
        public override void Step()
        {
            if (orderData == null)
            {
                SelfTerminate();
                return;
            }
            
            Vector3 relativePosition = orderData.position - unit.transform.position;
            
            if (relativePosition.magnitude < moveOrderExecutionModule.stoppingDistance)
            {
                SelfTerminate();
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, moveOrderExecutionModule.movementSpeed * Time.deltaTime);

            unit.transform.position += relativePosition.normalized * movementDistance;
        }
    }
}