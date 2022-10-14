using System;
using System.Collections.Generic;
using Systems.Interfaces;
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
        public Order order;
        public Transform targetTransform;
        public OrderType orderType;
        public bool groundOrder;
        public Vector3 position;
        public RtsAgent owner;
        
        public OrderData(Order order, Transform targetTransform, OrderType orderType, bool groundOrder, Vector3 position, RtsAgent owner)
        {
            this.order = order;
            this.targetTransform = targetTransform;
            this.orderType = orderType;
            this.groundOrder = groundOrder;
            this.position = position;
            this.owner = owner;

            if (!groundOrder)
            {
                targetTransform.GetComponent<IDestroyable>().OnDestroyableDestroy += HandleOrderDestroyed;
            }
            order.GetComponent<IDestroyable>().OnDestroyableDestroy += HandleOrderDestroyed;
        }
        
        public Vector3 GetOrderPosition()
        {
            return groundOrder ? position : targetTransform.position;
        }

        public void HandleOrderDestroyed(IDestroyable destroyable)
        {
            if (!groundOrder)
            {
                targetTransform.GetComponent<IDestroyable>().OnDestroyableDestroy -= HandleOrderDestroyed;
            }

            order.GetComponent<IDestroyable>().OnDestroyableDestroy -= HandleOrderDestroyed;
            owner.orderManager.DestroyOrder(this);
        }
    }

    public class Order : MonoBehaviour, IDestroyable
    {
        public event Action<IDestroyable> OnDestroyableDestroy;
        
        [SerializeField] private Renderer ren;

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
        
        private void OnDestroy()
        {
            OnDestroyableDestroy?.Invoke(this);
        }
        
        private void Update()
        {
            if (orderData.targetTransform != null)
            {
                transform.position = orderData.GetOrderPosition();
            }
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}