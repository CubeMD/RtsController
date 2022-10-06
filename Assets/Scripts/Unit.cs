using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Modules;
using Systems.Orders;
using Templates;
using Templates.Modules;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public event Action<Unit> OnUnitDestroyed;
    
    [SerializeField] private LineRenderer lineRenderer;

    public RtsAgent Owner { get; private set; }
    
    private UnitTemplate unitTemplate;
    private readonly List<Module> unitModules = new List<Module>();
    private readonly Dictionary<OrderType, List<OrderExecutionModule>> orderExecutionModulesByOrderType 
        = new Dictionary<OrderType, List<OrderExecutionModule>>();
    public Order activeOrder;

    private void OnDestroy()
    {
        if (activeOrder != null)
        {
            activeOrder.UnAssignUnit(this);
            //Owner.OrderGraph.RemoveUnitFromAllTransitions(this);
        }
        
        OnUnitDestroyed?.Invoke(this);
    }

    public void SetUnitTemplate(UnitTemplate template, RtsAgent unitOwner)
    {
        Owner = unitOwner;
        unitTemplate = template;
        
        foreach (ModuleTemplate moduleTemplate in unitTemplate.moduleTemplates)
        {
            if (moduleTemplate != null)
            {
                Module module = moduleTemplate.GetModule(this);
                unitModules.Add(module);
                
                if (module is OrderExecutionModule orderExecutionModule)
                {
                    IEnumerable<Enum> orderTypes = Enum.GetValues(typeof(OrderType)).Cast<Enum>()
                        .Where(x => !Equals((int)(object)x, 0) && orderExecutionModule.orderType.HasFlag(x));
                    
                    foreach (OrderType orderType in orderTypes)
                    {
                        if (!orderExecutionModulesByOrderType.ContainsKey(orderType))
                        {
                            orderExecutionModulesByOrderType.Add(orderType, new List<OrderExecutionModule>{orderExecutionModule});
                        }
                        else
                        {
                            orderExecutionModulesByOrderType[orderType].Add(orderExecutionModule);
                        }
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
        foreach (Module unitModule in unitModules)
        {
            unitModule.Update();
        }

        List<Vector3> points = new List<Vector3> { transform.position + Vector3.up};

        if (activeOrder != null)
        {
            Order currentOrder = activeOrder;
            points.Add(currentOrder.transform.position);
            
            while (true)
            {
                if (Owner.OrderGraph.TryGetTransition(this, out Transition transition, currentOrder))
                {
                    currentOrder = transition.nextOrder;
                    points.Add(currentOrder.transform.position + Vector3.up);
                }
                else
                {
                    break;
                }
            } 
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    
    public bool TryAssignActiveOrder(Order order)
    {
        if (orderExecutionModulesByOrderType.TryGetValue(order.OrderData.orderType, out List<OrderExecutionModule> orderExecutionModules))
        {
            activeOrder = order;
            
            activeOrder.AssignUnit(this);
            
            foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
            {
                orderExecutionModule.SetOrder(activeOrder);
            }

            return true;
        }

        return false;
    }

    public bool CanExecuteOrderType(OrderType orderType)
    {
        return orderExecutionModulesByOrderType.ContainsKey(orderType);
    }

    public void UnAssignActiveOrder()
    {
        if (activeOrder != null && 
            orderExecutionModulesByOrderType.TryGetValue(activeOrder.OrderData.orderType, out List<OrderExecutionModule> orderExecutionModules))
        {
            foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
            {
                orderExecutionModule.UnSetOrder();
            }

            activeOrder.UnAssignUnit(this);
            activeOrder = null;
        }
    }
    
    public void HandleUnitCompletedOrder()
    {
        Owner.OrderComplete();
        Owner.OrderGraph.TransitionUnitToNextOrder(this);
    }


    // public Order CreateSubOrder(Order parentOrder, OrderData orderData, Vector3 worldPosition)
    // {
    //     if (TryGetOrderExecutionModule(orderData.orderType, out OrderExecutionModule orderExecutionModule))
    //     {
    //         Order order = Owner.CreateOrder(orderData, Owner.transform.parent, Owner.transform.parent.InverseTransformPoint(worldPosition));
    //         
    //         order.AddAssignedUnit(this, true);
    //         
    //         ExecutingOrderState executingOrderState = orderExecutionModule.GetState(this, order);
    //         
    //         unitStateMachine.AddState(executingOrderState, true);
    //
    //         scheduledOrders.Add(order, executingOrderState);
    //         
    //         return order;
    //     }
    //     
    //     Debug.LogError("Could not create sub order");
    //     return null;
    // }
}