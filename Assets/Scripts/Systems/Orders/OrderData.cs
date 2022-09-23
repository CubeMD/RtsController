using UnityEngine;

namespace Systems.Orders
{
    public abstract class OrderData
    {
        public OrderType orderType;
        public bool subOrder;
    }
    
    public class MoveData : OrderData
    {
        public Vector3 position;

        public MoveData(Vector3 position, bool subOrder = false)
        {
            this.position = position;
            orderType = OrderType.Move;
            this.subOrder = subOrder;

        }  
    }
    
    public class AttackData : OrderData
    {
        public Unit unit;
            
        public AttackData(Unit unit)
        {
            this.unit = unit;
            orderType = OrderType.Attack;
        }
    }
    
    public class ReclaimData : OrderData
    {
        public readonly Reclaim reclaim;
            
        public ReclaimData(Reclaim reclaim)
        {
            this.reclaim = reclaim;
            orderType = OrderType.Reclaim;
        }
    }
}