using Systems.Orders;
using Templates.Modules;
using UnityEngine;

namespace Systems.Modules
{
    public class ReclaimModule : OrderExecutionModule
    {
        private float reclaimRange;
        private float reclaimPower;
        private OrderData reclaimOrderData;
        private Reclaim reclaim;

        public ReclaimModule(ReclaimModuleTemplate reclaimModuleTemplate, Unit unit) : base(unit)
        {
            orderType = reclaimModuleTemplate.orderType;
            reclaimRange = reclaimModuleTemplate.defaultReclaimRange;
            reclaimPower = reclaimModuleTemplate.defaultReclaimPower;
        }

        public override void SetOrder(Order order)
        {
            base.SetOrder(order);
            
            reclaimOrderData = order.OrderData;
            reclaim = order.OrderData.targetTransform.GetComponent<Reclaim>();
            reclaim.OnReclaimDestroyed += HandleReclaimDestroyed;
        }

        public override void UnSetOrder()
        {
            base.UnSetOrder();
            reclaim = null;
            reclaimOrderData = null;
        }

        public override void Update()
        {
            if (active)
            {
                Vector3 reclaimOffset = reclaim.transform.position - unit.transform.position;

                if (reclaimOffset.magnitude - reclaimRange <= 0)
                {
                    float amount = reclaimPower * Time.deltaTime;
                    unit.Owner.UnitCollectedMass(amount);
                    reclaim.Amount -= amount;
                }
            }
        }

        private void HandleReclaimDestroyed(Reclaim reclaim)
        {
            reclaim.OnReclaimDestroyed -= HandleReclaimDestroyed;
            Complete();
        }
    }
}