using System.Collections.Generic;
using Systems.Modules;
using Systems.Orders;
using UnityEngine;

namespace Systems.StateMachine.States
{
    public class ExecutingReclaimOrderState : ExecutingOrderState
    {
        private ReclaimData reclaimData;
        private ReclaimOrderExecutionModule reclaimOrderExecutionModule;
        private const float SUB_ORDER_SAFETY_DISTANCE = 0.1f;
        
        public ExecutingReclaimOrderState(Unit unit, Order order, ReclaimOrderExecutionModule reclaimOrderExecutionModule)
            : base(unit, order)
        {
            this.reclaimOrderExecutionModule = reclaimOrderExecutionModule;
            this.reclaimData = order.OrderData as ReclaimData;
        }
        
        public override void Step()
        {
            if (reclaimData.reclaim == null)
            {
                Complete();
                return;
            }

            Vector3 reclaimOffset = reclaimData.reclaim.transform.position - unit.transform.position;

            float distanceToBeAbleToReclaim = reclaimOffset.magnitude - reclaimOrderExecutionModule.reclaimRange;

            if (distanceToBeAbleToReclaim <= 0)
            {
                reclaimData.reclaim.Amount -= reclaimOrderExecutionModule.reclaimPower * Time.deltaTime;
            }
            else
            {
                Vector3 moveOrderDestination = unit.transform.position + reclaimOffset.normalized * (distanceToBeAbleToReclaim + SUB_ORDER_SAFETY_DISTANCE);

                unit.CreateSubOrder(order, new MoveData(moveOrderDestination, true), moveOrderDestination);
            }
        }
    }
}