using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Interfaces;
using Systems.Modules;
using Systems.Orders;
using Templates;
using Templates.Modules;
using UnityEngine;

public class Unit : MonoBehaviour, IDestroyable
{
    public event Action<IDestroyable> OnDestroyableDestroy;

    [SerializeField] private LineRenderer lineRenderer;

    public RtsAgent Owner { get; private set; }
    
    private UnitTemplate unitTemplate;
    private Environment environment;
    private OrderData activeOrderData;

    private readonly Dictionary<OrderType, List<OrderExecutionModule>> orderExecutionModulesTable 
        = new Dictionary<OrderType, List<OrderExecutionModule>>();

    private void OnDestroy()
    {
        OnDestroyableDestroy?.Invoke(this);
        Owner.orderManager.RemoveUnitFromGraph(this);
        Owner.ownedUnits.Remove(this);

        if (Owner.selectedUnits.Contains(this))
        {
            Owner.selectedUnits.Remove(this);
        }
    }

    public void SetUnitTemplate(UnitTemplate template, RtsAgent unitOwner, Environment environment)
    {
        Owner = unitOwner;
        unitTemplate = template;
        this.environment = environment;
        environment.OnEnvironmentReset += HandleEnvironmentReset;
        Owner.orderManager.AddUnitToGraph(this);
        Owner.ownedUnits.Add(this);
        
        foreach (ModuleTemplate moduleTemplate in unitTemplate.moduleTemplates)
        {
            if (moduleTemplate != null)
            {
                Module module = moduleTemplate.GetModule(this);

                if (module is OrderExecutionModule orderExecutionModule)
                {
                    IEnumerable<Enum> orderTypes = Enum.GetValues(typeof(OrderType)).Cast<Enum>()
                        .Where(x => !Equals((int)(object)x, 0) && orderExecutionModule.orderType.HasFlag(x));
                    
                    foreach (OrderType orderType in orderTypes)
                    {
                        if (!orderExecutionModulesTable.ContainsKey(orderType))
                        {
                            orderExecutionModulesTable.Add(orderType, new List<OrderExecutionModule>{orderExecutionModule});
                        }
                        else
                        {
                            orderExecutionModulesTable[orderType].Add(orderExecutionModule);
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
        if (activeOrderData != null)
        {
            orderExecutionModulesTable[activeOrderData.orderType].ForEach(x => x.Update());
            RenderOrderLines();
        }
    }
    
    public bool CanExecuteOrderType(OrderType orderType)
    {
        return orderExecutionModulesTable.ContainsKey(orderType);
    }

    public void AssignActiveOrder(OrderData orderData)
    {
        if (orderData != null)
        {
            orderExecutionModulesTable.TryGetValue(orderData.orderType,
                out List<OrderExecutionModule> orderExecutionModules);
            
            activeOrderData = orderData;
    
            foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
            {
                orderExecutionModule.SetActiveOrder(activeOrderData);
                SubscribeToOrderExecutionModule(orderExecutionModule);
            }
        }
    }
    
    public void UnAssignActiveOrder()
    {
        if (activeOrderData != null && 
            orderExecutionModulesTable.TryGetValue(activeOrderData.orderType, out List<OrderExecutionModule> orderExecutionModules))
        {
            foreach (OrderExecutionModule orderExecutionModule in orderExecutionModules)
            {
                orderExecutionModule.ClearActiveOrder();
            }
            
            activeOrderData = null;
        }
    }

    public void SubscribeToOrderExecutionModule(OrderExecutionModule module)
    {
        module.OnOrderCompleted += HandleOrderCompleted;
        module.OnOrderDestroyed += HandleOrderDestroyed;
    }

    private void HandleOrderDestroyed(OrderExecutionModule module, OrderData orderData)
    {
        TerminateOrder(module, orderData);
    }

    public void HandleOrderCompleted(OrderExecutionModule module, OrderData orderData)
    {
        TerminateOrder(module, orderData);
    }

    private void TerminateOrder(OrderExecutionModule module, OrderData previousOrderData)
    {
        module.OnOrderCompleted -= HandleOrderCompleted;
        module.OnOrderDestroyed -= HandleOrderDestroyed;
        
        Owner.OrderComplete(previousOrderData, this);
        UnAssignActiveOrder();
        if (Owner.orderManager.TryTransitionUnitToNextOrder(this, out OrderData orderData))
        {
            AssignActiveOrder(orderData);
        }
    }

    public void HandleEnvironmentReset()
    {
        Destroy(gameObject);
        environment.OnEnvironmentReset -= HandleEnvironmentReset;
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public void UnitCollectedMass(float amount)
    {
        Owner.UnitCollectedMass(amount);
    }

    private void RenderOrderLines()
    {
        List<Vector3> points = new List<Vector3>{transform.position + Vector3.up};

        if (activeOrderData != null)
        {
            points.Add(activeOrderData.position + Vector3.up);
        }
        
        Owner.orderManager.GetTransitionsForUnit(this).ForEach(x => points.Add(x.position + Vector3.up));
        
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}