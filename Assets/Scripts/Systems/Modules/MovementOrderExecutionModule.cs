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

        public MovementOrderExecutionModule(MovementModuleTemplate movementModuleTemplate, Unit unit) : base(unit)
        {
            orderType = movementModuleTemplate.orderType;
            movementSpeed = movementModuleTemplate.defaultMovementSpeed;
            stoppingDistance = Random.Range(0, 5);
        }

        public override void SetActiveOrder(OrderData activeOrderData)
        {
            base.SetActiveOrder(activeOrderData);
            
            if (activeOrderData.groundOrder)
            {
                followingTarget = false;
                targetTransform = null;
            }
            else
            {
                followingTarget = true;
                targetTransform = activeOrderData.targetTransform;
            }
        }

        public override void Update()
        {
            if (followingTarget && targetTransform == null)
            {
                BroadcastOrderCompleted();
                return;
            }
                
            Vector3 relativePosition = orderData.GetOrderPosition() - unit.transform.position;
            
            //TODO: Go over types and assign behaviour to type
            
            if (relativePosition.magnitude <= stoppingDistance)
            {
                if (!followingTarget)
                {
                    BroadcastOrderCompleted();
                }
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);

            unit.transform.position += relativePosition.normalized * movementDistance;
        }
    }
}
