using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    
    public class Order : MonoBehaviour
    {
        [SerializeField] private Renderer ren;

        public Transform targetTransform;
        public OrderType orderType;
        public List<Unit> assignedUnits;
        public bool groundOrder;
        public Vector3 position;
        public RtsAgent owner;
        private IDestroyable destroyable;
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void SetOrder(Transform targetTransform, OrderType orderType, List<Unit> assignedUnits, bool groundOrder, Vector3 position, RtsAgent owner, bool additive)
        {
            this.targetTransform = targetTransform;
            this.orderType = orderType;
            this.assignedUnits = assignedUnits;
            this.groundOrder = groundOrder;
            this.position = position;
            this.owner = owner;

            if (!groundOrder)
            {
                destroyable = targetTransform.GetComponent<IDestroyable>();
                destroyable.OnDestroyableDestroy += HandleOrderDependencyDestroyed;
            }

            foreach (Unit assignedUnit in assignedUnits)
            {
                assignedUnit.AssignNewOrder(this, additive);
            }
        }

        public void UnAssignUnit(Unit unit)
        {
            assignedUnits.Remove(unit);

            if (assignedUnits.Count < 1)
            {
                Destroy(gameObject);
            }
        }

        private void HandleOrderDependencyDestroyed(IDestroyable destroyable)
        {
            Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            if (destroyable != null)
            {
                destroyable.OnDestroyableDestroy -= HandleOrderDependencyDestroyed;
            }

            foreach (Unit assignedUnit in assignedUnits.ToList())
            {
                assignedUnit.UnAssignOrder(this);
            }
        }
        
        public Vector3 GetOrderPosition()
        {
            return groundOrder ? position : targetTransform.position;
        }
        
        private void Update()
        {
            if (!groundOrder)
            {
                transform.position = GetOrderPosition();
            }
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}