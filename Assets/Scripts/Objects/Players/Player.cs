using System.Collections.Generic;
using System.Linq;
using Objects.Agents;
using Objects.Orders;
using Systems.Interfaces;
using Systems.Templates;
using Tools.Utilities;
using UnityEngine;

namespace Objects.Players
{
    public abstract class Player : MonoBehaviour
    {
        public UnitTemplate factoryTemplate;
        public Transform cursorTransform;
        public Order orderPrefab;
        public Environment environment;
        public LayerMask interactableLayerMask;
        public int teamId;
    
        public readonly List<Unit> ownedUnits = new List<Unit>();
        public readonly List<Unit> selectedUnits = new List<Unit>();
        private readonly List<AgentStep> episodeTrajectory = new List<AgentStep>();
    
        public virtual void ResetPlayer()
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
            ResetPlayer();
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
        
        public void CreateOrder(OrderType orderType, Transform targetTransform, List<Unit> capableUnits, bool additive, Vector3 localOffset = new Vector3())
        {
            Order order = ObjectPooler.InstantiateGameObject(orderPrefab, targetTransform.position + localOffset, Quaternion.identity, environment.transform);

            order.SetOrder(targetTransform, orderType, capableUnits, targetTransform.position + localOffset, additive);

            // foreach (Unit capableUnit in capableUnits)
            // {
            //     if (capableUnit.assignedOrders.Any(x => x.targetTransform != null && x.targetTransform == hitInfo.transform))
            //     {
            //         AddReward(-1f);
            //     }
            // }
        }
        
        public Unit SpawnUnit(UnitTemplate unitTemplate, Vector3 position)
        {
            Unit unit = ObjectPooler.InstantiateGameObject(environment.unitPrefab, environment.transform.position + position, Quaternion.identity, transform);
            unit.SetUnitTemplate(unitTemplate, this);
            unit.OnDestroyableDestroy += HandleUnitDestroyed;
            ownedUnits.Add(unit);
            return unit;
        }
    }
}
