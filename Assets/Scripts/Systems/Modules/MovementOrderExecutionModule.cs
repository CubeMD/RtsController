using Objects;
using Objects.Orders;
using Systems.Templates.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Modules
{
    public class MovementOrderExecutionModule : OrderExecutionModule
    {
        private float movementSpeed;
        private float stoppingDistance;
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
            targetTransform = order.targetTransform;
        }

        public override void Update()
        {
            if (targetTransform == null)
            {
                Debug.Log("This should not be the case");
                return;
            }

            Vector3 relativePosition = executedOrder.GetOrderPosition() - unit.transform.position;

            if (relativePosition.magnitude <= stoppingDistance)
            {
                OrderCompleted();
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);

            unit.transform.position += relativePosition.normalized * movementDistance;
        }
    }
}
