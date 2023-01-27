using Orders;
using Units.UnitStates;
using Units.UnitStates.UnitStateParameters;
using UnityEngine;

namespace Units.LandUnits
{
    public class Commander : MovableUnit
    {
        public ReclaimingUnitParameters reclaimingUnitParameters;

        public override void StartExecutingFirstOrder()
        {
            base.StartExecutingFirstOrder();

            if (assignedOrders.Count > 0)
            {
                if (assignedOrders[0].orderType == OrderType.Move)
                {
                    stateMachine.queuedStates.Add(new UnitMovingState(assignedOrders[0].Position, this));
                }
                else if (assignedOrders[0].orderType == OrderType.Reclaim)
                {
                    stateMachine.queuedStates.Add(new UnitReclaimingState(reclaimingUnitParameters, 
                        (assignedOrders[0] as TargetedOrder).targetTransform.GetComponent<Reclaim>(), 
                        this));
                }
            }
        }
    }
}