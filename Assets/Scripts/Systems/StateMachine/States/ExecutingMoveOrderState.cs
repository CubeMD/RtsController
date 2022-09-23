using Systems.Modules;
using Systems.Orders;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class ExecutingMoveOrderState : ExecutingOrderState
    {

        private MoveData orderData;
        private MoveOrderExecutionModule moveOrderExecutionModule;
        
        public ExecutingMoveOrderState(Unit unit, Order order, MoveOrderExecutionModule moveOrderExecutionModule)
            : base(unit, order)
        {
            this.moveOrderExecutionModule = moveOrderExecutionModule;
            orderData = order.OrderData as MoveData;
        }
        
        public override void Step()
        {
            Vector3 relativePosition = orderData.position - unit.transform.position;
            
            if (relativePosition.magnitude <= float.Epsilon)
            {
                Complete();
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, moveOrderExecutionModule.movementSpeed * Time.deltaTime);

            unit.transform.position += relativePosition.normalized * movementDistance;
        }
    }
}