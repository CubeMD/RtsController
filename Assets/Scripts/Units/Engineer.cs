using Orders;
using Targets;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

namespace Units
{
    public class Engineer : LandUnit
    {
        [SerializeField] private float reclaimRange;
        [SerializeField] private float buildPower;
        
        public override void Reclaim(ReclaimOrderState reclaimOrderState)
        {
            float distanceToReclaim = (reclaimOrderState.reclaim.transform.position - transform.position).magnitude;
            
            if (distanceToReclaim < reclaimRange)
            {
                reclaimOrderState.reclaim.Reward -= buildPower * Time.deltaTime;
            }
        }

        public override void ContextualAction(RaycastHit hitInfo, bool additive)
        {
            base.ContextualAction(hitInfo, additive);
            
            if (hitInfo.collider.TryGetComponent(out Reclaim reclaim))
            {
                if (additive)
                {
                    orderStateMachine.QueueState(new ReclaimOrderState(this, reclaim));
                }
                else
                {
                    orderStateMachine.SetActiveState(new ReclaimOrderState(this, reclaim));
                }
            }
        }
    }
}