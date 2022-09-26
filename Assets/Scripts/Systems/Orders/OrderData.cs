using UnityEngine;

namespace Systems.Orders
{
    public abstract class OrderData
    {
        public OrderType orderType;
        public MonoBehaviour parentObject;
    }
    
    public class MoveData : OrderData
    {
        public Vector3 position;

        public MoveData(Vector3 position, Order parentOrder = null)
        {
            this.position = position;
            orderType = OrderType.Move;
            parentObject = parentOrder;
        }  
    }
    
    public class AttackData : OrderData
    {
        public Unit unit;
            
        public AttackData(Unit unit)
        {
            this.unit = unit;
            orderType = OrderType.Attack;
            parentObject = unit;
        }
    }
    
    public class ReclaimData : OrderData
    {
        public readonly Reclaim reclaim;
            
        public ReclaimData(Reclaim reclaim)
        {
            this.reclaim = reclaim;
            orderType = OrderType.Reclaim;
            parentObject = reclaim;
        }
    }
}