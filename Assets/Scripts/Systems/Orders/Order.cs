using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Orders
{
    public class Order : MonoBehaviour
    {
        public OrderType orderType;
        public OrderData orderData;
        
        public abstract class OrderData
        {
            
        }
        
        public class ReclaimData : OrderData
        {
            public Reclaim reclaim;
            
            public ReclaimData(Reclaim reclaim)
            {
                this.reclaim = reclaim;
            }
        }
        
        public class MoveData : OrderData
        {
            public Vector3 position;
            
            public MoveData(Vector3 position)
            {
                this.position = position;
            }
        }
        
        public class AttackData : OrderData
        {
            public Unit unit;
            
            public AttackData(Unit unit)
            {
                this.unit = unit;
            }
        }
        
        private readonly List<Unit> assignedUnits = new List<Unit>();

        public List<Unit> GetAssignedUnits()
        {
            return assignedUnits;
        }

        public void AddAssignedUnit(Unit unit)
        {
            assignedUnits.Add(unit);
        }

        public void AddAssignedUnit(IEnumerable<Unit> units)
        {
            assignedUnits.AddRange(units);
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);
                return true;
            }
            
            return false;
        }

        public void SelfDestroy()
        {
            Destroy(this);
        }

        private void OnDestroy()
        {
            foreach (Unit assignedUnit in assignedUnits)
            {
                assignedUnit.UnAssignOrder(this);
            }
        }
    }

    [Flags]
    public enum OrderType
    {
        Move,
        Attack,
        Reclaim,
        Build,
        Capture
    }
}