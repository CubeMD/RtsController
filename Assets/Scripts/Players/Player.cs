using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Agents;
using Systems.Interfaces;
using Systems.Orders;
using Tools;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public Transform cursorTransform;
    public Order orderPrefab;
    public Environment environment;
    public LayerMask interactableLayerMask;
    
    public readonly List<Unit> ownedUnits = new List<Unit>();
    public readonly List<Unit> selectedUnits = new List<Unit>();
    
    private readonly List<AgentStep> episodeTrajectory = new List<AgentStep>();
    
    public virtual void Reset()
    {
        ownedUnits.Clear();
        selectedUnits.Clear();
        episodeTrajectory.Clear();
    }
    
    public virtual void UnitCollectedMass(float amount)
    {
    }
    
    public virtual void HandleEnvironmentReset()
    {
        Reset();
    }

    public virtual void AddReward(float amount)
    {
        
    }
    
    public void HandleUnitDestroyed(IDestroyable destroyable)
    {
        Unit unit = destroyable.GetGameObject().GetComponent<Unit>();
        
        unit.OnDestroyableDestroy -= HandleUnitDestroyed;

        ownedUnits.Remove(unit);

        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
        }
    }

    public void CreateAndAssignOrder(RaycastHit hitInfo, List<Unit> assignedUnits, bool additive)
    {
        OrderType orderType;
        bool groundOrder = false;
        Vector3 groundHitPosition = hitInfo.point;

        if (hitInfo.collider.TryGetComponent(out Reclaim reclaim))
        {
            orderType = OrderType.Reclaim;
        }
        else if (hitInfo.collider.TryGetComponent(out Unit unit) && !ownedUnits.Contains(unit))
        {
            orderType = OrderType.Attack;
        }
        else
        {
            orderType = OrderType.Move;
            groundOrder = true;
        }

        List<Unit> capableUnits = assignedUnits.Where(unit => unit.CanExecuteOrderType(orderType)).ToList();

        if (capableUnits.Count < 1) return;
        
        Order order = ObjectPooler.InstantiateGameObject(orderPrefab, groundHitPosition, Quaternion.identity, transform.parent);

        order.SetOrder(hitInfo.transform, orderType, capableUnits, groundOrder, groundHitPosition, this, additive);

        foreach (Unit capableUnit in capableUnits)
        {
            if (capableUnit.assignedOrders.Any(x => x.targetTransform != null && x.targetTransform == hitInfo.transform))
            {
                AddReward(-1f);
            }
        }
    }
}
