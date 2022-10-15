using Systems.Orders;
using Templates.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Modules
{
    public class MovementOrderExecutionModule : OrderExecutionModule
    {
        public float movementSpeed;
        private float stoppingDistance;
        private bool followingTarget;
        private Transform targetTransform;

        public MovementOrderExecutionModule(MovementOrderExecutionModuleTemplate movementOrderExecutionModuleTemplate, Unit unit) : base(unit)
        {
            orderType = movementOrderExecutionModuleTemplate.orderType;
            movementSpeed = movementOrderExecutionModuleTemplate.speed;
            stoppingDistance = Random.Range(0, 2.5f);
        }

        public override void SetExecutedOrder(Order order)
        {
            base.SetExecutedOrder(order);
            
            if (order.groundOrder)
            {
                followingTarget = false;
                targetTransform = null;
            }
            else
            {
                followingTarget = true;
                targetTransform = order.targetTransform;
            }
        }

        public override void Update()
        {
            if (followingTarget && targetTransform == null)
            {
                OrderCompleted();
                return;
            }

            Vector3 relativePosition = executedOrder.GetOrderPosition() - unit.transform.position;
            
            //TODO: Go over types and assign behaviour to type
            
            if (relativePosition.magnitude <= stoppingDistance)
            {
                if (!followingTarget)
                {
                    OrderCompleted();
                }
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);

            unit.transform.position += relativePosition.normalized * movementDistance;
        }
    }
}
