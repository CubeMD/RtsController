using System.Collections.Generic;
using System.Linq;
using Objects.Orders;
using Tools.Utilities;
using UnityEngine;

namespace Objects.Players
{
    public class HumanPlayer : Player
    {
        [SerializeField] private Transform selectionBoxTransform;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraGizmoTransform;

        private float CameraZoom
        {
            get => cam.orthographicSize;
            set
            {
                cam.orthographicSize = value;
                Vector3 gizmoScale = Vector3.one * (value * 2);
                gizmoScale.y = 1;
                cameraGizmoTransform.localScale = gizmoScale;
            }
        }
        
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Confined;
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

            //order.SetOrder(hitInfo.transform, orderType, capableUnits, groundOrder, groundHitPosition, this, additive);

            // foreach (Unit capableUnit in capableUnits)
            // {
            //     if (capableUnit.assignedOrders.Any(x => x.targetTransform != null && x.targetTransform == hitInfo.transform))
            //     {
            //         AddReward(-1f);
            //     }
            // }
        }
        
        // private void OnDestroy()
        // {
        //     CameraZoom = zoomMinMax.x;
        //     cameraTransform.localPosition = Vector3.zero;
        //     cursorTransform.localPosition = Vector3.zero;
        //     selectionBoxTransform.localScale = Vector3.zero;
        //     selectionBoxTransform.localPosition = Vector3.zero;
        //
        // }
        
//         private void DumpTrajectoryIntoHeuristicAndEndEpisode()
//         {
//             int count = episodeTrajectory.Count - 1;
//             for (int index = 0; index < count; index++)
//             {
//                 RequestDecision();
// //            Academy.Instance.EnvironmentStep();
//                 episodeTrajectory.RemoveAt(0);
//             }
//             
//             EndEpisode();
//         }
        
        private void Update()
    {
        // public void FixedUpdate()
        // {
        //
        //
        //     if (isHuman)
        //     {
        //         // if (timeSinceLastDecision == 0)
        //         // {
        //         //     completedRtsAgentStep.rtsAgentObservation = GetAgentEnvironmentObservation();
        //         // }
        //         //
        //         // timeSinceLastDecision += Time.fixedDeltaTime;
        //         //
        //         // if (timeSinceLastDecision >= delayMax || completedRtsAgentStep.rtsAgentAction != null)
        //         // {
        //         //     DetailedDebug($"Action recorded in fixed update at time {Time.time}");
        //         //     
        //         //     Vector3 humanCursorPositionRelativeToCamera = cameraTransform.InverseTransformPoint(cam.ScreenToWorldPoint(Input.mousePosition));
        //         //     Vector2 cursorAction = new Vector2(
        //         //         Mathf.Clamp(humanCursorPositionRelativeToCamera.x / CameraZoom, -1f, 1), 
        //         //         Mathf.Clamp(humanCursorPositionRelativeToCamera.z / CameraZoom, -1f, 1));
        //         //     
        //         //     completedRtsAgentStep.rtsAgentAction ??= new RtsAgentAction(cursorAction, timeSinceLastDecision, AgentActionType.None, false, 0);
        //         //     
        //         //     // // Regularize continuous actions
        //         //     // AddReward(-0.005f);
        //         //     // AddReward(-0.005f * completedRtsAgentStep.rtsAgentAction.currentCursorAction.magnitude);
        //         //     // AddReward(completedRtsAgentStep.rtsAgentAction.currentZoomAction != 0 ? -0.01f : 0f);
        //         //     
        //         //     completedRtsAgentStep.reward = GetCumulativeReward();
        //         //     episodeTrajectory.Add(completedRtsAgentStep);
        //         //     
        //         //     InteractWithEnvironment(completedRtsAgentStep.rtsAgentAction);
        //         //     
        //         //     timeSinceLastDecision = 0;
        //         //     completedRtsAgentStep = new AgentStep();
        //         // }
        //     }
        
        //Zoom camera and correct mouse
        // if (rtsAgentAction.currentZoomAction != 0)
        // {
        //     float clampedZoomOffset = Mathf.Clamp(rtsAgentAction.currentZoomAction * zoomSpeed, zoomMinMax.x - CameraZoom, zoomMinMax.y - CameraZoom);
        //
        //     if (clampedZoomOffset < 0)
        //     {
        //         cameraTransform.localPosition += cursorTransform.localPosition * ((1 - Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom + clampedZoomOffset)) * 0.4f);
        //     }
        //     
        //     cursorTransform.localPosition *= (CameraZoom + clampedZoomOffset) / CameraZoom;
        //     CameraZoom += clampedZoomOffset;
        // }
        //
        // // Move Camera
        // Vector3 desiredCameraOffset = new Vector3(
        //     cur.x > 0 ? Mathf.Min(cursorOffsetCorrection.x, 0) : Mathf.Max(cursorOffsetCorrection.x, 0),
        //     0,
        //     cur.z > 0 ? Mathf.Min(cursorOffsetCorrection.z, 0) : Mathf.Max(cursorOffsetCorrection.z, 0));
        //
        // Vector3 desiredCameraPosition = cameraTransform.localPosition - desiredCameraOffset;
        //     
        // cameraTransform.localPosition = new Vector3(
        //     Mathf.Clamp(desiredCameraPosition.x, -MapSize + CameraZoom, MapSize - CameraZoom),
        //     0f,
        //     Mathf.Clamp(desiredCameraPosition.z, -MapSize + CameraZoom, MapSize - CameraZoom));
        
        // selectionBoxTransform.localScale = Vector3.zero;
        // selectionBoxTransform.localPosition = Vector3.zero;
        // lastClickWorldPos = cursorTransform.position;
        
        // if (rtsAgentAction.currentAgentActionType == AgentActionType.LeftDrag)
        // {
        //     // Left click
        //     Vector3 cursorOffset = cursorTransform.position - lastClickWorldPos;
        //     selectionBoxTransform.localScale = new Vector3(Mathf.Abs(cursorOffset.x), 1, Mathf.Abs(cursorOffset.z));
        //     selectionBoxTransform.localPosition = cursorTransform.localPosition - cursorOffset / 2;
        //
        //     if (!rtsAgentAction.currentShiftAction)
        //     {
        //         selectedUnits.Clear();
        //     }
        //
        //     foreach (Collider col in Physics.OverlapBox(selectionBoxTransform.position,
        //                  selectionBoxTransform.localScale / 2,
        //                  Quaternion.identity,
        //                  interactableLayerMask))
        //     {
        //         if (col.TryGetComponent(out Unit unit) && ownedUnits.Contains(unit) &&
        //             !selectedUnits.Contains(unit))
        //         {
        //             selectedUnits.Add(unit);
        //         }
        //     }
        // }
        
        // DetailedDebug($"Update at time {Time.time}");
        //
        // if (!isHuman || 
        //     !Application.isFocused ||
        //     IsMouseOutOfScreen() || 
        //     completedRtsAgentStep.rtsAgentAction != null) return;
        //
        // if (Input.mouseScrollDelta.y != 0 && !humanIsHoldingLeftMouseButton)
        // {
        //     completedRtsAgentStep.rtsAgentAction ??= new RtsAgentAction
        //     {
        //         currentZoomAction = -Input.mouseScrollDelta.y < 0 ? -1 : 1
        //     };
        // }
        // else if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     humanIsHoldingLeftMouseButton = true;
        //     
        //     completedRtsAgentStep.rtsAgentAction ??= new RtsAgentAction
        //     {
        //         currentAgentActionType = AgentActionType.None
        //     };
        // }
        // else if (Input.GetKeyUp(KeyCode.Mouse0))
        // {
        //     humanIsHoldingLeftMouseButton = false;
        //     
        //     completedRtsAgentStep.rtsAgentAction ??= new RtsAgentAction
        //     {
        //         currentAgentActionType = AgentActionType.LeftDrag
        //     };
        // }
        // else if (Input.GetKeyDown(KeyCode.Mouse1) && !humanIsHoldingLeftMouseButton)
        // {
        //     completedRtsAgentStep.rtsAgentAction ??= new RtsAgentAction
        //     {
        //         currentAgentActionType = AgentActionType.RightClick
        //     };
        // }
        //
        // if (completedRtsAgentStep.rtsAgentAction != null)
        // {
        //     DetailedDebug($"Action recorded in update at time {Time.time}");
        //     Vector3 humanCursorPositionRelativeToCamera = cameraTransform.InverseTransformPoint(cam.ScreenToWorldPoint(Input.mousePosition));
        //     
        //     completedRtsAgentStep.rtsAgentAction.currentCursorAction = new Vector2(
        //         Mathf.Clamp(humanCursorPositionRelativeToCamera.x / CameraZoom, -1f, 1), 
        //         Mathf.Clamp(humanCursorPositionRelativeToCamera.z / CameraZoom, -1f, 1));
        //     
        //     completedRtsAgentStep.rtsAgentAction.timeForScheduledDecision = timeSinceLastDecision;
        //     
        //     if (Input.GetKey(KeyCode.LeftShift))
        //     {
        //         completedRtsAgentStep.rtsAgentAction.currentShiftAction = true;
        //     }
        // }
    }
        
        private bool IsMouseOutOfScreen()
        {
            return Input.mousePosition.x < 0 ||
                   Input.mousePosition.x > 1000 ||
                   Input.mousePosition.y < 0 ||
                   Input.mousePosition.y > 1000;
        }
    }
}