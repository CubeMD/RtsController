using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Modules;
using Systems.Orders;
using Systems.StateMachine;
using Systems.StateMachine.States;
using Templates;
using Templates.Modules;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public event Action<Unit> OnUnitDestroyed;
    
    [SerializeField] private UnitTemplate unitTemplate;
    [SerializeField] private StateMachine unitStateMachine;
    [SerializeField] private LineRenderer lineRenderer;
    
    public RtsAgent Owner { get; private set; }

    private readonly List<Module> unitModules = new List<Module>();
    
    private readonly Dictionary<OrderType, OrderExecutionModule> availableOrders 
        = new Dictionary<OrderType, OrderExecutionModule>();

    private readonly Dictionary<Order, ExecutingOrderState> scheduledOrders =
        new Dictionary<Order, ExecutingOrderState>();

    private void OnDestroy()
    {
        OnUnitDestroyed?.Invoke(this);
        
        UnAssignAllOrders();
    }

    private void Update()
    {
        foreach (Order order in scheduledOrders.Keys.ToList())
        {
            //line
        }
    }

    public void SetUnit(UnitTemplate template, RtsAgent unitOwner)
    {
        Owner = unitOwner;
        unitTemplate = template;
        
        foreach (ModuleTemplate moduleTemplate in unitTemplate.moduleTemplates)
        {
            if (moduleTemplate != null)
            {
                Module module = moduleTemplate.GetModule();
                unitModules.Add(module);
                
                if (module is OrderExecutionModule orderExecutionModule)
                {
                    availableOrders.Add(orderExecutionModule.orderType, orderExecutionModule);
                }
            }
            else
            {
                Debug.LogWarning($"Module template in {unitTemplate.name} is empty");
            }
        }
    }
    
    public bool TryGetOrderExecutionModule(OrderType orderType, out OrderExecutionModule orderExecutionModule)
    {
        return availableOrders.TryGetValue(orderType, out orderExecutionModule);
    }
    
    public bool TryAssignOrder(Order order, bool additive, bool subOrder = false)
    {
        if (TryGetOrderExecutionModule(order.OrderData.orderType, out OrderExecutionModule orderExecutionModule))
        {
            ExecutingOrderState executingOrderState = orderExecutionModule.GetState(this, order);
            
            if (!additive)
            {
                UnAssignAllOrders();
            }

            unitStateMachine.AddState(executingOrderState, subOrder);

            scheduledOrders.Add(order, executingOrderState);
            
            return true;
        }

        return false;
    }

    public bool TryUnAssignOrder(Order order)
    {
        while (scheduledOrders.ContainsKey(order))
        {
            unitStateMachine.TryRemoveState(scheduledOrders[order]);

            scheduledOrders.Remove(order);

            order.TryRemoveUnit(this);
            return true;
        }

        return false;
    }

    public void UnAssignAllOrders()
    {
        foreach (Order order in scheduledOrders.Keys.ToList())
        {
            TryUnAssignOrder(order);
        }
    }

    public Order CreateSubOrder(Order parentOrder, OrderData orderData, Vector3 worldPosition)
    {
        if (TryGetOrderExecutionModule(orderData.orderType, out OrderExecutionModule orderExecutionModule))
        {
            Order order = Owner.CreateOrder(orderData, Owner.transform.parent, Owner.transform.parent.InverseTransformPoint(worldPosition));
            
            order.AddAssignedUnit(this, true);
            
            ExecutingOrderState executingOrderState = orderExecutionModule.GetState(this, order);
            
            unitStateMachine.AddState(executingOrderState, true);

            scheduledOrders.Add(order, executingOrderState);
            
            return order;
        }
        
        Debug.LogError("Could not create sub order");
        return null;
    }
}