﻿using Systems.Interfaces;
using Systems.Orders;
using Templates.Modules;
using UnityEngine;

namespace Systems.Modules
{
    public class ReclaimOrderExecutionModule : OrderExecutionModule
    {
        private float reclaimRange;
        private float reclaimPower;
        private Reclaim targetReclaim;

        public ReclaimOrderExecutionModule(EngineeringOrderExecutionModuleTemplate engineeringOrderExecutionModuleTemplate, Unit unit) : base(unit)
        {
            orderType = engineeringOrderExecutionModuleTemplate.orderType;
            reclaimRange = engineeringOrderExecutionModuleTemplate.range;
            reclaimPower = engineeringOrderExecutionModuleTemplate.power;
        }

        public override void SetExecutedOrder(Order order)
        {
            base.SetExecutedOrder(order);
            
            if (order.targetTransform.TryGetComponent(out Reclaim reclaim))
            {
                targetReclaim = reclaim;
            }
            else
            {
                Debug.Log("ReclaimExecMod SetOrder");
            }
        }

        public override void ClearActiveOrder()
        {
            base.ClearActiveOrder();
            targetReclaim = null;
        }

        public override void Update()
        {
            Vector3 reclaimOffset = targetReclaim.transform.position - unit.transform.position;

            if (reclaimOffset.magnitude - reclaimRange <= 0)
            {
                float potentialReclaim = reclaimPower * Time.deltaTime;

                float remainingReclaim = Mathf.Max(targetReclaim.Amount - potentialReclaim, 0);
                
                float reclaimAmount = targetReclaim.Amount - remainingReclaim;
                
                targetReclaim.Amount = remainingReclaim;
                
                unit.owner.UnitCollectedMass(reclaimAmount);
                
                if (remainingReclaim <= 0)
                {
                    OrderCompleted();
                }
            }
        }
    }
}