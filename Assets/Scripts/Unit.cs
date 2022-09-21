using System.Collections.Generic;
using Systems.Modules;
using Systems.Orders;
using Systems.StateMachine;
using Systems.StateMachine.States;
using Templates;
using Templates.Modules;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitTemplate unitTemplate;

    [SerializeField] private StateMachine unitStateMachine;

    private List<Module> unitModules = new List<Module>();

    private Dictionary<OrderType, OrderExecutionModule> availableOrders = new Dictionary<OrderType, OrderExecutionModule>();

    public void SetUnitTemplate(UnitTemplate template)
    {
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
    
    public bool TryAssignOrder(Order order, bool additive)
    {
        if (TryGetOrderExecutionModule(order.orderType, out OrderExecutionModule orderExecutionModule))
        {
            ExecutingOrderState executingOrderState = orderExecutionModule.GetState(this, order);
            
            if (additive)
            {
                unitStateMachine.QueueState(executingOrderState);
            }
            else
            {
                unitStateMachine.SetActiveState(executingOrderState);
            }

            return true;
        }

        return false;
    }
    

    
    public void TerminateCurrentOrder()
    {
            
    }

    public void TerminateAllOrders()
    {
            
    }

    public void UnAssignOrder(Order order)
    {
        //TODO: implement
    }
}