using Systems.Interfaces;
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

        public ReclaimOrderExecutionModule(ReclaimModuleTemplate reclaimModuleTemplate, Unit unit) : base(unit)
        {
            orderType = reclaimModuleTemplate.orderType;
            reclaimRange = reclaimModuleTemplate.defaultReclaimRange;
            reclaimPower = reclaimModuleTemplate.defaultReclaimPower;
        }

        public override void SetActiveOrder(OrderData activeOrderData)
        {
            base.SetActiveOrder(activeOrderData);
            
            targetReclaim = activeOrderData.targetTransform.GetComponent<Reclaim>();
            targetReclaim.OnDestroyableDestroy += HandleTargetTargetReclaimDestroyed;
        }

        private void HandleTargetTargetReclaimDestroyed(IDestroyable obj)
        {
            BroadcastOrderCompleted();
        }
        
        public override void Update()
        {
            Vector3 reclaimOffset = targetReclaim.transform.position - unit.transform.position;

            if (reclaimOffset.magnitude - reclaimRange <= 0)
            {
                float amount = Mathf.Min(targetReclaim.Amount - reclaimPower * Time.deltaTime, 0);
                targetReclaim.Amount -= amount;
                unit.UnitCollectedMass(amount);
            }
        }
    }
}