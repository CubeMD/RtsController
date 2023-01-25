using System;
using System.Collections.Generic;
using Interfaces;
using Orders;
using Players;
using UnityEngine;
using Utilities;
using Utilities.StateMachine;

namespace Units
{
    public enum UnitType
    {
        Commander,
        Engineer,
        Tank,
        Factory
    }
    
    public class Unit : MonoBehaviour, IDestroyable
    {
        public event Action<IDestroyable> OnDestroyableDestroy;
        
        public Player owner;
        public StateMachine stateMachine = new StateMachine();
        public OrderType orderCapability;
        public UnitType unitType;
        
        [SerializeField] private Mesh mesh;
        [SerializeField] private Renderer render;
        [SerializeField] private bool renderOrderLines;
        [SerializeField] private LineRenderer lineRenderer;

        public readonly List<Order> assignedOrders = new List<Order>();

        public void Update()
        {
            stateMachine.Step();

            if (renderOrderLines)
            {
                RenderOrderLines();
            }
        }

        public bool CanExecuteOrderType(OrderType orderType)
        {
            return orderCapability.HasFlag(orderType);
        }
        
        public void DestroyUnit()
        {
            UnAssignUnitFromAllOrders();
            OnDestroyableDestroy?.Invoke(this);
            ObjectPooler.PoolGameObject(gameObject);
        }
        
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void UnAssignUnitFromAllOrders()
        {
            foreach (Order assignedOrder in assignedOrders)
            {
                assignedOrder.assignedUnits.Remove(this);
            }
            
            assignedOrders.Clear();
        }
        
        public void AssignOrder(Order order, bool additive)
        {
            if (!additive)
            {
                UnAssignUnitFromAllOrders();
            }
        
            assignedOrders.Add(order);

            if (assignedOrders.Count == 1)
            {
                StartExecutingOrder(assignedOrders[0]);
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
        
        public virtual void StartExecutingOrder(Order order)
        {

        }
        
        public void TransitionToNextOrder()
        {
            assignedOrders[0].assignedUnits.Remove(this);
            assignedOrders.RemoveAt(0);
            StartExecutingOrder(assignedOrders[0]);
        }
        
        private void RenderOrderLines()
        {
            List<Vector3> points = new List<Vector3>{transform.position + Vector3.up};

            assignedOrders.ForEach(x => points.Add(x.Position + Vector3.up));

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}