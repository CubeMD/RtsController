using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Objects.Players;
using Systems.Interfaces;
using Tools.Utilities;
using UnityEngine;

namespace Objects.Orders
{
    [Flags]
    public enum OrderType
    {
        Move = 1,
        Attack = 2,
        Reclaim = 4,
        Assist = 8,
        BuildTankUnit = 16,
        BuildEngineerUnit = 32,
        BuildFactoryUnit = 64
    }
    
    public class Order : MonoBehaviour
    {
        [SerializeField] private Renderer ren;

        public Transform targetTransform;
        public OrderType orderType;
        public List<Unit> assignedUnits;
        public Vector3 position;
        private IDestroyable destroyable;
        
        public void SetOrder(Transform targetTransform, OrderType orderType, List<Unit> assignedUnits, Vector3 position, bool additive)
        {
            this.targetTransform = targetTransform;
            this.orderType = orderType;
            this.assignedUnits = assignedUnits;
            this.position = position;

            if (orderType != OrderType.Move && orderType != OrderType.BuildFactoryUnit)
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
                ObjectPooler.PoolGameObject(gameObject);
            }
        }

        private void HandleOrderDependencyDestroyed(IDestroyable destroyable)
        {
            //owner.AddReward(-0.1f);
            ObjectPooler.PoolGameObject(gameObject);
        }
        
        private void OnDisable()
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
            return orderType == OrderType.Move || orderType == OrderType.BuildFactoryUnit ? position : targetTransform.position;
        }
        
        private void Update()
        {
            if (orderType == OrderType.Move && orderType == OrderType.BuildFactoryUnit)
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