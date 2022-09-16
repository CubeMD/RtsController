using Orders;
using Targets;
using UnityEngine;

namespace Units
{
    public class LandUnit : Unit
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float stoppingDistance;

        protected override void Move(MoveOrderState moveOrderState)
        {
            Vector3 relativePosition = moveOrderState.PositionToFollow - transform.position;
            
            if (relativePosition.magnitude < stoppingDistance)
            {
                moveOrderState.();
                return;
            }

            float movementDistance = Mathf.Clamp(relativePosition.magnitude, 0, movementSpeed * Time.deltaTime);

            transform.position += relativePosition.normalized * movementDistance;
        }

        public override void ContextualAction(RaycastHit hitInfo, bool additive)
        {

            // else if (hitInfo.collider.TryGetComponent(out Unit unit))
            // {
            //     if (additive)
            //     {
            //         unitSm.QueueState(new TransformMoveOrderState(this, unit.transform));
            //     }
            //     else
            //     {
            //         unitSm.SetActiveState(new TransformMoveOrderState(this, unit.transform));
            //     }
            // }
            
            if (hitInfo.collider.CompareTag("Ground"))
            {
                Vector3 hitLocation = new Vector3(hitInfo.point.x, 0, hitInfo.point.z);
                
                if (additive)
                {
                    orderStateMachine.QueueState(new PositionMoveOrderState(this, hitLocation));
                }
                else
                {
                    orderStateMachine.SetActiveState(new PositionMoveOrderState(this, hitLocation));
                }
            }
        }
    }
}