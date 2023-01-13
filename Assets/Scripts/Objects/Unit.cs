using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Orders;
using Objects.Players;
using Systems.Interfaces;
using Systems.Modules;
using Systems.Templates;
using Systems.Templates.Modules;
using Tools.Utilities;
using UnityEngine;

namespace Objects
{
    public class Unit : MonoBehaviour, IDestroyable
    {
        public event Action<IDestroyable> OnDestroyableDestroy;

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private bool renderOrderLines;
    
        public Player owner;
        public UnitTemplate unitTemplate;
        private Environment environment;
        public readonly List<Order> assignedOrders = new List<Order>();

        public readonly Dictionary<OrderType, List<OrderExecutionModule>> orderTypeExecutionModulesTable 
            = new Dictionary<OrderType, List<OrderExecutionModule>>();

        private void OnDisable()
        {
            OnDestroyableDestroy?.Invoke(this);

            foreach (Order assignedOrder in assignedOrders)
            {
                assignedOrder.UnAssignUnit(this);
            }
        }

        public void SetUnitTemplate(UnitTemplate template, Player unitOwner)
        {
            owner = unitOwner;
            unitTemplate = template;
        
            environment = unitOwner.environment;
            environment.OnEnvironmentReset += HandleEnvironmentReset;
        
            foreach (OrderExecutionModuleTemplate orderExecutionModuleTemplate in unitTemplate.orderExecutionModuleTemplates)
            {
                if (orderExecutionModuleTemplate != null)
                {
                    OrderExecutionModule orderExecutionModule = orderExecutionModuleTemplate.GetOrderExecutionModule(this);
                
                    foreach (OrderType orderType in GetFlags(orderExecutionModule.orderType))
                    {
                        if (!orderTypeExecutionModulesTable.ContainsKey(orderType))
                        {
                            orderTypeExecutionModulesTable.Add(orderType, new List<OrderExecutionModule>{orderExecutionModule});
                        }
                        else
                        {
                            orderTypeExecutionModulesTable[orderType].Add(orderExecutionModule);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Module template in {unitTemplate.name} is empty");
                }
            }
        }

        public void Update()
        {
            if (assignedOrders.Count > 0)
            {
                ExecuteCurrentOrder();
            }
        
            if (renderOrderLines)
            {
                RenderOrderLines();
            }
        }

        public void ExecuteCurrentOrder()
        {
            orderTypeExecutionModulesTable[assignedOrders[0].orderType].ForEach(x => x.Update());
        }
    
        public bool CanExecuteOrderType(OrderType orderType)
        {
            return orderTypeExecutionModulesTable.ContainsKey(orderType);
        }

        public void AssignNewOrder(Order order, bool additive)
        {
            if (!additive)
            {
                UnAssignUnitFromAllOrders();
            }
        
            assignedOrders.Add(order);

            if (assignedOrders.Count == 1)
            {
                StartExecutingFirstAssignedOrder();
            }
        }

        public void UnAssignOrder(Order order)
        {
            if (assignedOrders.IndexOf(order) == 0)
            {
                TransitionToNextOrder();
            }
            else
            {
                assignedOrders.Remove(order);
            }
        }

        public void StartExecutingFirstAssignedOrder()
        {
            if (assignedOrders.Count > 0)
            {
                if (orderTypeExecutionModulesTable.TryGetValue(assignedOrders[0].orderType, out List<OrderExecutionModule> orderExecutionModules))
                {
                    foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
                    {
                        orderExecutionModule.SetExecutedOrder(assignedOrders[0]);
                        orderExecutionModule.OnOrderExecutionModuleCompletedOrder += HandleOrderExecutionModuleCompletedOrder;
                    }
                }
            }
        }
    
        public void StopExecutingFirstAssignedOrder()
        {
            if (assignedOrders.Count > 0)
            {
                if (orderTypeExecutionModulesTable.TryGetValue(assignedOrders[0].orderType, out List<OrderExecutionModule> orderExecutionModules))
                {
                    foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
                    {
                        orderExecutionModule.ClearActiveOrder();
                        orderExecutionModule.OnOrderExecutionModuleCompletedOrder -= HandleOrderExecutionModuleCompletedOrder;
                    }
                }
            }
        }

        public void TransitionToNextOrder()
        {
            StopExecutingFirstAssignedOrder();
            assignedOrders[0].UnAssignUnit(this);
            assignedOrders.RemoveAt(0);
            StartExecutingFirstAssignedOrder();
        }
    
        public void UnAssignUnitFromAllOrders()
        {
            StopExecutingFirstAssignedOrder();

            foreach (Order assignedOrder in assignedOrders)
            {
                //owner.AddReward(-1f);
                assignedOrder.UnAssignUnit(this);
            }
        
            assignedOrders.Clear();
        }

        public void HandleOrderExecutionModuleCompletedOrder(Unit unit, Order order)
        {
            TransitionToNextOrder();
        }

        private void HandleEnvironmentReset()
        {
            environment.OnEnvironmentReset -= HandleEnvironmentReset;
            ObjectPooler.PoolGameObject(gameObject);
        }
    
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    
        private void RenderOrderLines()
        {
            List<Vector3> points = new List<Vector3>{transform.position + Vector3.up};

            assignedOrders.ForEach(x => points.Add(x.position + Vector3.up));

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum input)
        {
            return Enum.GetValues(input.GetType()).Cast<Enum>().Where(input.HasFlag);
        }
    }
}