using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Orders
{
    [Flags]
    public enum OrderType
    {
        Move = 1,
        Attack = 2,
        Reclaim = 4,
        Build = 8,
        Capture = 16
    }
    
    public abstract class OrderData
    {
        public OrderType orderType;
    }
    
    public class MoveOrderData : OrderData
    {
        public virtual Vector3 Position { get; }

        public MoveOrderData(Vector3 position)
        {
            Position = position;
            orderType = OrderType.Move;
        }  
    }

    public class MoveTargetOrderData : MoveOrderData
    {
        private readonly Transform targetTransform;
        public override Vector3 Position => targetTransform.position;

        public MoveTargetOrderData(Transform transform) : base(transform.position)
        {
            targetTransform = transform;
        }
    }
    
    public class AttackOrderData : OrderData
    {
        public readonly Unit unit;
            
        public AttackOrderData(Unit unit)
        {
            this.unit = unit;
            orderType = OrderType.Attack;
        }
    }
    
    public class ReclaimOrderData : OrderData
    {
        public readonly Reclaim reclaim;
            
        public ReclaimOrderData(Reclaim reclaim)
        {
            this.reclaim = reclaim;
            orderType = OrderType.Reclaim;
        }
    }
    
    public class Order : MonoBehaviour
    {
        [SerializeField] private Renderer ren;

        public List<Unit> assignedUnits = new List<Unit>();

        public RtsAgent owner;

        private OrderData orderData;
        public OrderData OrderData
        {
            get => orderData;
            
            set
            {
                orderData = value;
                
                if (orderData.orderType == OrderType.Move)
                {
                    ren.material.color = Color.cyan;
                }
                else if (orderData.orderType == OrderType.Reclaim)
                {
                    ren.material.color = Color.yellow;
                }
                else if (orderData.orderType == OrderType.Attack)
                {
                    ren.material.color = Color.red;
                }
                
            }
        }

        public void OnDestroy()
        {
            owner.OrderGraph.RemoveOrder(this);
        }

        public void AssignUnit(Unit unit)
        {
            assignedUnits.Add(unit);
        }

        public void UnAssignUnit(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);

                CheckHasAssignedUnits();
            }
        }

        /// <summary>
        /// TODO: THIS IS BAD. VERY VERY VERY BAD
        /// </summary>
        /// <returns></returns>
        public bool CheckHasAssignedUnits()
        {
            if (assignedUnits.Count < 1 && !owner.OrderGraph.HasTransitionsToOrder(this))
            {
                SelfDestroy();
                return false;
            }

            return true;
        }

        public void SelfDestroy()
        {
            Destroy(gameObject);
        }
    }
}