using Systems.Modules;
using Systems.Orders;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class ExecutingReclaimOrderState : ExecutingOrderState
    {
        private Order.ReclaimData reclaimData;
        private ReclaimOrderExecutionModule reclaimOrderExecutionModule;
        
        public ExecutingReclaimOrderState(Unit unit, Order order, ReclaimOrderExecutionModule reclaimOrderExecutionModule)
            : base(unit, order)
        {
            this.reclaimOrderExecutionModule = reclaimOrderExecutionModule;
            this.reclaimData = order.orderData as Order.ReclaimData;
        }
        
        public override void Step()
        {
            if (reclaimData == null)
            {
                SelfTerminate();
                return;
            }

            if (reclaimData.reclaim == null)
            {
                order.SelfDestroy();
                SelfTerminate();
                return;
            }
            
            float distanceToReclaim = (reclaimData.reclaim.transform.position - unit.transform.position).magnitude;
            
            if (distanceToReclaim < reclaimOrderExecutionModule.reclaimRange)
            {
                reclaimData.reclaim.Amount -= reclaimOrderExecutionModule.reclaimPower * Time.deltaTime;
            }
        }
    }
}