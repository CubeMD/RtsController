using System;
using System.Collections.Generic;
using Interfaces;
using Units;
using UnityEngine;

namespace Orders
{
    [Flags]
    public enum OrderType
    {
        Move = 1,
        Attack = 2,
        Reclaim = 4,
        Assist = 8,
        BuildTank = 16,
        BuildEngineer = 32,
        BuildFactory = 64,
    }
    
    public class Order
    {
        public OrderType orderType;
        public readonly List<Unit> assignedUnits;
        
        private Vector3 position;
        public virtual Vector3 Position => position;

        public Order(OrderType orderType, List<Unit> assignedUnits, Vector3 position)
        {
            this.orderType = orderType;
            this.assignedUnits = assignedUnits;
            this.position = position;
        }
    }

    public class TargetedOrder : Order
    {
        public readonly Transform targetTransform;
        public override Vector3 Position => targetTransform.position;
        
        public TargetedOrder(OrderType orderType, List<Unit> assignedUnits, Transform targetTransform) : base(orderType, assignedUnits, targetTransform.position)
        {
            this.targetTransform = targetTransform;
            targetTransform.GetComponent<IDestroyable>().OnDestroyableDestroy += HandleOrderDependencyDestroyed;
        }

        private void HandleOrderDependencyDestroyed(IDestroyable dependencyDestroyable)
        {
            dependencyDestroyable.OnDestroyableDestroy -= HandleOrderDependencyDestroyed;

            int numUnits = assignedUnits.Count;

            for (int i = 0; i < numUnits; i++)
            {
                assignedUnits[0].UnAssignOrder(this);
            }
        }
    }
}