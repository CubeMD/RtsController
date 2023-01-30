using System;
using Interfaces;
using Players;
using Systems.StateMachine;
using Units.States;
using Units.States.UnitStateParameters;
using UnityEngine;
using Utilities;

namespace Units
{
    [Flags]
    public enum OrderType
    {
        Move = 1,
        Attack = 2,
        Reclaim = 4,
        Assist = 8,
        BuildTank = 16,
        BuildEngineer = 32,
        BuildFactory = 64,
    }
    
    public enum UnitType
    {
        Commander,
        Engineer,
        Tank,
        Factory
    }
    
    [RequireComponent(typeof(StateMachine))]
    public class Unit : MonoBehaviour, IDestroyable
    {
        public event Action<IDestroyable> OnDestroyableDestroy;
        public event Action<Unit> OnUnitDestroyed;
        
        [SerializeField] protected StateMachine stateMachine;
        public StateMachine StateMachine => stateMachine;
        
        [SerializeField] private OrderType orderCapability;
        
        [SerializeField] private UnitType unitType;
        public UnitType UnitType => unitType;
        
        [SerializeField] private float size;
        public float Size => size;
        
        [SerializeField] private UnitStatParameters statParameters;

        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material disableMaterial;
        [SerializeField] private MeshRenderer meshRenderer;
        
        public Player Owner { get; private set; }
        public bool IsConstructed => statParameters.IsConstructed;
        
        public void SetOwner(Player owner)
        {
            Owner = owner;
        }
        
        protected void AssignState(State state, bool queue)
        {
            if (queue)
            {
                stateMachine.QueueState(state);
            }
            else
            {
                stateMachine.SetActiveState(state);
            }
        }
        
        public bool CanExecuteOrderType(OrderType orderType)
        {
            return IsConstructed && orderCapability.HasFlag(orderType);
        }

        public void DumpMass(float amount)
        {
            statParameters.DumpMass(amount);
        }

        public void StartConstruction()
        {
            AssignState(new ConstructionState(this, statParameters), false);    
        }
        
        public void ForceConstructed()
        {
            statParameters.SetConstructed();
        }
        
        public void DestroyUnit()
        {
            stateMachine.TerminateAllStates();
            OnDestroyableDestroy?.Invoke(this);
            ObjectPooler.PoolGameObject(gameObject);
            OnUnitDestroyed?.Invoke(this);
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public virtual void DisableUnit()
        {
            meshRenderer.material = disableMaterial;
        }

        public virtual void EnableUnit()
        {
            meshRenderer.material = activeMaterial;
        }
    }
}