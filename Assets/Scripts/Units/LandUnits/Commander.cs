using Orders;
using Units.UnitStates;
using UnityEngine;

namespace Units.LandUnits
{
    public class Commander : LandUnit
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float stoppingDistance;
        [SerializeField] private float reclaimRange;
        [SerializeField] private float reclaimPower;

        public override void StartExecutingOrder(Order order)
        {
            if (order.orderType == OrderType.Move)
            {
                stateMachine.currentState = new UnitMovingState(order.Position, movementSpeed, this);
            }
            else if (order.orderType == OrderType.Reclaim)
            {
                stateMachine.currentState = new UnitReclaimingState(reclaimRange, reclaimPower, this);
            }
        }
    }
}