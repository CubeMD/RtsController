using System;
using Economy;
using Interfaces;
using Players;
using Systems.StateMachine;
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
        [SerializeField] private OrderType orderCapability;
        [SerializeField] private UnitType unitType;
        public UnitType UnitType => unitType;
        public StateMachine StateMachine => stateMachine;
        
        public Player Owner { get; private set; }

        [SerializeField] private float size;

        public float Size => size;
        [SerializeField] private float energyCost;
        [SerializeField] private float massCost;
        [SerializeField] private float maxHp;
        
        [SerializeField] private float currentHp;

        public float CurrentHp
        {
            get => currentHp;
            set
            {
                if (value <= 0)
                {
                    DestroyUnit();
                }
                else
                {
                    currentHp = value;
                }
            }
        }

        [SerializeField] private float constructionPercentage;

        public bool IsConstructionComplete => constructionPercentage == 1f;

        public void SetOwner(Player owner)
        {
            this.Owner = owner;
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
            return IsConstructionComplete && orderCapability.HasFlag(orderType);
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

        public void ForceCompleteConstruction()
        {
            constructionPercentage = 1f;
            currentHp = maxHp;
        }

        public void Construct(EngineerParameters engineerParameters, EconomyManager economyManager)
        {
            constructionPercentage += 0.01f * Time.deltaTime;
            currentHp += 0.01f * Time.deltaTime;

            if (constructionPercentage >= 1f)
            {
                ForceCompleteConstruction();
            }
        }
    }
}