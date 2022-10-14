using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Interfaces;
using UnityEngine;

namespace Systems.Orders
{
    public class Link
    {
        public readonly OrderData nextOrder;
        public readonly List<Unit> assignedUnits;

        public Link(OrderData nextOrder, List<Unit> assignedUnits)
        {
            this.nextOrder = nextOrder;
            this.assignedUnits = assignedUnits;
        }
    }

    public class OrderChain
    {
        public readonly List<Link> links = new List<Link>();
    }
    
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private Order orderPrefab;
        [SerializeField] private RtsAgent owner;
        
        private readonly Dictionary<Unit, OrderChain> unitOrderChainTable = new Dictionary<Unit, OrderChain>();

        public void IssueOrderToSelectedUnits(RaycastHit hitInfo, List<Unit> assignedUnits, bool additive)
        {
            Transform targetTransform;
            OrderType orderType;
            bool groundOrder = false;
            Vector3 position;
            
            if (hitInfo.collider.TryGetComponent(out Reclaim reclaim))
            {
                orderType = OrderType.Reclaim;
                targetTransform = reclaim.transform;
                position = reclaim.transform.position;
            }
            else if (hitInfo.collider.TryGetComponent(out Unit unit) && !owner.ownedUnits.Contains(unit))
            {
                orderType = OrderType.Attack;
                targetTransform = unit.transform;
                position = unit.transform.position;
            }
            else
            {
                orderType = OrderType.Move;
                targetTransform = hitInfo.transform;
                groundOrder = true;
                position = hitInfo.point;
            }
            
            position.y = 0;

            List<Unit> capableUnits = assignedUnits.Where(unit => unit.CanExecuteOrderType(orderType)).ToList();

            if (capableUnits.Count < 1) return;

            Order order = Instantiate(orderPrefab, position, Quaternion.identity, transform);

            OrderData orderData = new OrderData(order, targetTransform, orderType, groundOrder, position, owner);
            
            order.OrderData = orderData;
            
            if (!additive)
            {
                RemoveUnitsFromAllTransitions(capableUnits);
            }
            
            CreateLink(orderData, capableUnits);
        }

        public void DestroyOrder(OrderData orderData)
        {
            if (orderData.order != null)
            {
                Destroy(orderData.order);
            }
            
            RemoveOrderData(orderData);
        }

        public void AddUnitToGraph(Unit unit)
        {
            unitOrderChainTable.Add(unit, new OrderChain());
        }

        public void RemoveUnitFromGraph(Unit unit)
        {
            RemoveUnitsFromAllTransitions(new List<Unit> { unit });
            unitOrderChainTable.Remove(unit);
        }
        
        public void CreateLink(OrderData orderData, List<Unit> units)
        {
            Link link = new Link(orderData, units);
            
            foreach (Unit unit in units)
            {
                if (unitOrderChainTable[unit].links.Count == 0)
                {
                    unit.AssignActiveOrder(orderData);
                }
                else
                {
                    unitOrderChainTable[unit].links.Add(link);
                }
            }
        }

        private void RemoveLink(Link link)
        {
            foreach (OrderChain orderChain in unitOrderChainTable.Values)
            {
                orderChain.links.Remove(link);
            }
        }        
        
        private Link FindLinksToOrderData(OrderData orderData)
        {
            foreach (OrderChain orderChain in unitOrderChainTable.Values)
            {
                foreach (Link link in orderChain.links.Where(x => x.nextOrder == orderData))
                {
                    return link;
                }
            }
            
            return null;
        }

        private void RemoveUnitsFromLink(List<Unit> units, Link link)
        {
            foreach (Unit unit in units)
            {
                link.assignedUnits.Remove(unit);
                unitOrderChainTable[unit].links.Remove(link);
            }

            if (link.assignedUnits.Count < 1)
            {
                RemoveLink(link);
            }
        }
        
        public void RemoveUnitsFromAllTransitions(List<Unit> units)
        {
            foreach (Unit unit in units)
            {
                for (int i = unitOrderChainTable[unit].links.Count - 1; i >= 0; i--)
                {
                    Link link = unitOrderChainTable[unit].links[i];
                    List<Unit> unitsToRemove = link.assignedUnits.Intersect(units).ToList();
                    RemoveUnitsFromLink(unitsToRemove, link); 
                }
            }
        }
        
        public void RemoveOrderData(OrderData orderData)
        {
            RemoveLink(FindLinksToOrderData(orderData));
        }
        
        public bool TryTransitionUnitToNextOrder(Unit unit, out OrderData orderData)
        {
            if (unitOrderChainTable[unit].links.Count > 0)
            {
                orderData = unitOrderChainTable[unit].links[0].nextOrder;
                RemoveUnitsFromLink(new List<Unit> { unit }, unitOrderChainTable[unit].links[0]);
                return true;
            }

            orderData = null;
            return false;
        }
        
        public List<OrderData> GetTransitionsForUnit(Unit unit)
        {
            return unitOrderChainTable[unit].links.Select(x => x.nextOrder).ToList();
        }
    }
}