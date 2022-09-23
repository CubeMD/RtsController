using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems.Orders
{
    [Flags]
    public enum OrderType
    {
        Move,
        Attack,
        Reclaim,
        Build,
        Capture
    }
    
    public class Order : MonoBehaviour
    {
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
            }
        }

        private readonly List<Unit> assignedUnits = new List<Unit>();

        public List<Unit> GetAssignedUnits()
        {
            return assignedUnits;
        }

        public void AddAssignedUnit(Unit unit, bool additive) // TODO: should not pass additive here?
        {
            if (unit.TryAssignOrder(this, additive, orderData.subOrder))
            {
                assignedUnits.Add(unit);
            }
        }

        public void AddAssignedUnit(IEnumerable<Unit> units, bool additive)
        {
            foreach (Unit unit in units)
            {
                AddAssignedUnit(unit, additive);
            }
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);

                CheckEmpty();
                return true;
            }
            
            return false;
        }

        public void CheckEmpty()
        {
            if (assignedUnits.Count == 0)
            {
                SelfDestroy();
            }
        }

        public void SelfDestroy()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            foreach (Unit assignedUnit in assignedUnits.ToList())
            {
                assignedUnit.TryUnAssignOrder(this);
            }
        }
    }
}