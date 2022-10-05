using System;
using Systems.Orders;
using Templates.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Modules
{
    public class MovementModule : OrderExecutionModule
    {
        public float movementSpeed;
        private MoveOrderData moveOrderData;
        private float stoppingDistance;
        private bool followingTarget;
        private Transform targetTransform;

        public MovementModule(MovementModuleTemplate movementModuleTemplate, Unit unit) : base(unit)
        {
            orderType = movementModuleTemplate.orderType;
            movementSpeed = movementModuleTemplate.defaultMovementSpeed;
            stoppingDistance = Random.Range(0, 2);
        }

        public override void SetOrder(Order order)
        {
            base.SetOrder(order);

            Type orderDataType = order.OrderData.GetType();
            
            if (orderDataType == typeof(MoveOrderData))
            {
                moveOrderData = order.OrderData as MoveOrderData;
                followingTarget = false;
                targetTransform = null;
            }
            else if (orderDataType == typeof(ReclaimOrderData))
            {
                ReclaimOrderData reclaimOrderData = order.OrderData as ReclaimOrderData;
                moveOrderData = new MoveTargetOrderData(reclaimOrderData.reclaim.transform);
                followingTarget = true;
                targetTransform = reclaimOrderData.reclaim.transform;
            }
            else if (orderDataType == typeof(AttackOrderData))
            {
                AttackOrderData attackOrderData = order.OrderData as AttackOrderData;
                moveOrderData = new MoveTargetOrderData(attackOrderData.unit.transform);
                followingTarget = true;
                targetTransform = attackOrderData.unit.transform;
            }
        }
        
        public override void UnSetOrder()
        {
            base.UnSetOrder();
            moveOrderData = null;
        }

        public override void Update()
        {
            if (active)
            {
                if (followingTarget && targetTransform == null)
                {
                    Complete();
                    return;
                }
                
                Vector3 relativePosition = moveOrderData.Position - unit.transform.position;
            
                if (relativePosition.magnitude <= stoppingDistance)
                {
                    if (!followingTarget)
                    {
                        Complete();
                    }
                    return;
                }

                float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);

                unit.transform.position += relativePosition.normalized * movementDistance;
            }
        }
    }
}
