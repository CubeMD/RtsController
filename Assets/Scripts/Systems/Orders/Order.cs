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
    
    public class OrderData
    {
        public Transform targetTransform;
        public OrderType orderType;
        
        public OrderData(Transform targetTransform, OrderType orderType)
        {
            this.targetTransform = targetTransform;
            this.orderType = orderType;
        }
        
        public virtual Vector3 GetOrderPosition()
        {
            return targetTransform.position;
        }
    }

    public class PositionOrderData : OrderData
    {
        public Vector3 targetPosition;

        public PositionOrderData(Vector3 targetPosition, Transform targetTransform, OrderType orderType) : base(targetTransform, orderType)
        {
            this.targetPosition = targetPosition;
        }
        
        public override Vector3 GetOrderPosition()
        {
            return targetPosition;
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

        private void Update()
        {
            if (orderData.targetTransform != null)
            {
                transform.position = orderData.GetOrderPosition();
            }
            else
            {
                SelfDestroy();
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