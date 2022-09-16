using Orders;
using Systems.StateMachine;
using UnityEngine;

namespace Units
{
    public abstract class Unit : MonoBehaviour
    {
        private readonly StateMachine unitStateMachine = new StateMachine();

        public virtual void Reclaim(ReclaimOrderState reclaimOrderState)
        {
            
        }

        public virtual void Move(MoveOrderState moveOrderState)
        {
            
        }

        public virtual void Build()
        {
            
        }

        public virtual void Attack()
        {
            
        }

        public virtual void ContextualAction(RaycastHit hitInfo, bool additive)
        {
        }

        public virtual void Order(OrderState orderState, bool additive)
        {
            if (additive)
            {
                
            }
        }
    }
}