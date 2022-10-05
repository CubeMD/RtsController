using Systems.Orders;
using Templates.Modules;
using UnityEngine;

namespace Systems.Modules
{
    public class ReclaimModule : OrderExecutionModule
    {
        private float reclaimRange;
        private float reclaimPower;
        private ReclaimOrderData reclaimOrderData;

        public ReclaimModule(ReclaimModuleTemplate reclaimModuleTemplate, Unit unit) : base(unit)
        {
            orderType = reclaimModuleTemplate.orderType;
            reclaimRange = reclaimModuleTemplate.defaultReclaimRange;
            reclaimPower = reclaimModuleTemplate.defaultReclaimPower;
        }

        public override void SetOrder(Order order)
        {
            base.SetOrder(order);
            reclaimOrderData = order.OrderData as ReclaimOrderData;
        }

        public override void UnSetOrder()
        {
            base.UnSetOrder();
            reclaimOrderData = null;
        }

        public override void Update()
        {
            if (active)
            {
                if (reclaimOrderData.reclaim == null)
                {
                    Complete();
                    return;
                }

                Vector3 reclaimOffset = reclaimOrderData.reclaim.transform.position - unit.transform.position;

                if (reclaimOffset.magnitude - reclaimRange <= 0)
                {
                    reclaimOrderData.reclaim.Amount -= reclaimPower * Time.deltaTime;
                }
            }
        }
    }
}