using System;
using System.Collections.Generic;
using System.Linq;
using AgentDebugTool.Scripts.Agent;
using Systems.Interfaces;
using Systems.Orders;
using Templates;
using Tools;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class RtsAgent : DebuggableAgent
{
    private enum ActionType
    {
        MoveCamera = 0,
        LeftDrag = 1,
        RightClick = 2,
        None = 3
    }

    [Header("Objects")]
    [SerializeField] private BufferSensorComponent reclaimSensorComponent;
    [SerializeField] private BufferSensorComponent unitSensorComponent;
    [SerializeField] private BufferSensorComponent orderSensorComponent;
    [SerializeField] public Environment environment;
    [SerializeField] public Order orderPrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraGizmoTransform;
    [SerializeField] private Transform cursorTransform;
    [SerializeField] private Transform selectionBoxTransform;
    
    [Header("Unit")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private UnitTemplate startingUnitTemplate;
    [SerializeField] private int numStartingUnits = 1;
    [SerializeField] private float startingUnitSpread = 20;
    
    [Header("Parameters")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Vector2 zoomMinMax;
    [SerializeField] private bool writeHeuristic;
    [SerializeField] private bool drawBufferSensorMonitor;
    [SerializeField] private LayerMask interactableLayerMask;

    public readonly List<Unit> ownedUnits = new List<Unit>();
    public readonly List<Unit> selectedUnits = new List<Unit>();
    
    private bool primaryActionHasBeenRecorded;
    private Vector2 recordedCursorAction;
    private float recordedZoomAction;
    private ActionType recordedPrimaryAction = ActionType.None;
    private bool recordedShiftAction;

    private float CameraZoom
    {
        get => cam.orthographicSize;
        set
        {
            cam.orthographicSize = value;
            Vector3 gizmoScale = Vector3.one * value * 2;
            gizmoScale.y = 1;
            cameraGizmoTransform.localScale = gizmoScale;
        }
    }

    private float MapSize => environment.halfGroundSize;
    
    public override void OnEpisodeBegin()
    {
        CameraZoom = zoomMinMax.x;
        cameraTransform.localPosition = Vector3.zero;
        cursorTransform.localPosition = Vector3.zero;
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;
        
        ownedUnits.Clear();
        selectedUnits.Clear();

        primaryActionHasBeenRecorded = false;
        recordedCursorAction = Vector2.zero;
        recordedPrimaryAction = ActionType.None;
        recordedShiftAction = false;

        SpawnStartingUnits();
    }
    
    private void SpawnStartingUnits()
    {
        for (int i = 0; i < numStartingUnits; i++)
        {
            Vector3 localPosition = new Vector3(
                Random.Range(-startingUnitSpread, startingUnitSpread), 
                0, 
                Random.Range(-startingUnitSpread, startingUnitSpread));

            SpawnUnit(startingUnitTemplate, localPosition);
        }
    }

    public void SpawnUnit(UnitTemplate unitTemplate, Vector3 localPosition)
    {
        Unit unit = Instantiate(unitPrefab, transform.parent.localPosition + localPosition, Quaternion.identity, transform.parent);
        unit.SetUnitTemplate(unitTemplate, this);
        unit.OnDestroyableDestroy += HandleUnitDestroyed;
        ownedUnits.Add(unit);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        Monitor.RemoveAllValuesFromAllTransforms();
        Vector3 camRelNormPos = cursorTransform.InverseTransformPoint(cameraTransform.position) / CameraZoom;
        Vector2 camPosObservation = new Vector2(camRelNormPos.x, camRelNormPos.z);
        sensor.AddObservation(camPosObservation);
        observationsDebugSet.Add("Cam Pos", $"{camPosObservation}");
        
        Vector2 cursorPosObservation =  new Vector2(
            (cameraTransform.localPosition.x + cursorTransform.localPosition.x) / MapSize, 
            (cameraTransform.localPosition.z + cursorTransform.localPosition.z) / MapSize);
        sensor.AddObservation(cursorPosObservation);
        observationsDebugSet.Add("Cursor Pos", $"{cursorPosObservation}");
        
        float zoomObservation = Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom);
        sensor.AddObservation(zoomObservation);
        observationsDebugSet.Add("Zoom", $"{zoomObservation}");
        
        float time = environment.timeSinceReset / environment.timeToReset;
        sensor.AddObservation(time);
        observationsDebugSet.Add("Time", $"{time}");

        foreach (Collider collider in Physics.OverlapBox(cameraTransform.position, Vector3.one * CameraZoom, Quaternion.identity, interactableLayerMask))
        {
            Vector3 relNormPos = cursorTransform.InverseTransformPoint(collider.transform.position) / MapSize;
            
            List<float> interactableObservation = new List<float>
            {
                relNormPos.x, 
                relNormPos.z,
            };
            
            if (collider.TryGetComponent(out Reclaim reclaim))
            {
                interactableObservation.Add(reclaim.Amount);
                reclaimSensorComponent.AppendObservation(interactableObservation.ToArray());
                
                if (drawBufferSensorMonitor)
                {

                    Monitor.Log("Data: ", string.Join(" ", interactableObservation), collider.transform);
                    Monitor.Log("Type: ", "Reclaim", collider.transform);
                }
            }
            else if (collider.TryGetComponent(out Unit unit))
            {
                interactableObservation.Add(selectedUnits.Contains(unit) ? 1 : -1);
                unitSensorComponent.AppendObservation(interactableObservation.ToArray());
                
                if (drawBufferSensorMonitor)
                {
                    Monitor.Log("Data: ", string.Join(" ", interactableObservation), collider.transform);
                    Monitor.Log("Type: ", "Unit", collider.transform);
                }
            }


        }

        foreach (Unit ownedUnit in ownedUnits)
        {
            for (int index = 0; index < ownedUnit.assignedOrders.Count; index++)
            {
                Vector3 relUnitNormPos = cursorTransform.InverseTransformPoint(ownedUnit.transform.position) / MapSize;
                Vector3 relOrderNormPos = cursorTransform.InverseTransformPoint(ownedUnit.assignedOrders[index].transform.position) / MapSize;
                
                List<float> orderObservation = new List<float>
                {
                    relUnitNormPos.x,
                    relUnitNormPos.z,
                    relOrderNormPos.x,
                    relOrderNormPos.z,
                    ownedUnit.assignedOrders[index].orderType == OrderType.Move ? 1 : 0,
                    ownedUnit.assignedOrders[index].orderType == OrderType.Reclaim ? 1 : 0,
                    index
                };

                Monitor.Log("Data: ", string.Join(" ", orderObservation), ownedUnit.assignedOrders[index].transform);
                Monitor.Log("Type: ", "Order", ownedUnit.assignedOrders[index].transform);

                orderSensorComponent.AppendObservation(orderObservation.ToArray());
            }
        }
        
        BroadcastObservationsCollected();
    }

    private void Update()
    {
        if (!writeHeuristic) return;
        
        recordedZoomAction = Mathf.Clamp(recordedZoomAction - Input.mouseScrollDelta.y / 3f, -1f, 1f);
        
        if (primaryActionHasBeenRecorded) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            recordedPrimaryAction = ActionType.None;
            primaryActionHasBeenRecorded = true;
        } 
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            recordedPrimaryAction = ActionType.LeftDrag;
            primaryActionHasBeenRecorded = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            recordedPrimaryAction = ActionType.RightClick;
            primaryActionHasBeenRecorded = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            recordedPrimaryAction = ActionType.MoveCamera;
            primaryActionHasBeenRecorded = true;
        }
        else
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            recordedShiftAction = true;
        }
        
        Vector3 cursorLocalPos = cursorTransform.transform.InverseTransformPoint(cam.ScreenToWorldPoint(new Vector3(
            Mathf.Clamp(Input.mousePosition.x, 0, 1000), 
            Mathf.Clamp(Input.mousePosition.y, 0, 1000),
            25)));
        
        recordedCursorAction = new Vector2(
            Mathf.Clamp(cursorLocalPos.x / CameraZoom / 2, -1f, 1),
            Mathf.Clamp(cursorLocalPos.z / CameraZoom / 2 , -1f, 1));
        
        //Debug.Log($"Heuristic action: {cursorAction}, {zoomAction}, {actionType}, {shiftAction}");
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        continuousActions[0] = recordedCursorAction.x;
        continuousActions[1] = recordedCursorAction.y;
        continuousActions[2] = recordedZoomAction;
        
        discreteActions[0] = (int)recordedPrimaryAction;
        discreteActions[1] = recordedShiftAction ? 1 : 0;
        
        primaryActionHasBeenRecorded = false;
        recordedCursorAction = Vector2.zero;
        recordedZoomAction = 0;
        recordedPrimaryAction = ActionType.None;
        recordedShiftAction = false;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        
        AddReward(-0.001f);
        
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;

        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        ActionType currentPrimaryAction = (ActionType)discreteActions[0];
        bool currentShiftAction = discreteActions[1] == 1;
        
        Vector2 currentCursorAction = 2 * new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));
        
        float currentZoomAction = Mathf.Clamp(continuousActions[2], -1f, 1f);
        
        // Regularize continuous actions
        AddReward(-0.01f * (currentCursorAction.magnitude * currentCursorAction.magnitude / 4f));
        AddReward(-0.01f * (currentZoomAction * currentZoomAction));
        
        if (currentPrimaryAction == ActionType.MoveCamera)
        {
            // Move Camera
            Vector3 desiredCameraLocalPosition = cameraTransform.localPosition + new Vector3(
                CameraZoom * currentCursorAction.x,
                0f,
                CameraZoom * currentCursorAction.y);
            
            cameraTransform.localPosition = new Vector3(
                Mathf.Clamp(desiredCameraLocalPosition.x, -MapSize + CameraZoom, MapSize - CameraZoom),
                0f,
                Mathf.Clamp(desiredCameraLocalPosition.z, -MapSize + CameraZoom, MapSize - CameraZoom));
        }
        else
        {
            // Move cursor
            Vector3 clampedCursorOffset = new Vector3(
                Mathf.Clamp(CameraZoom * currentCursorAction.x,
                    -CameraZoom - cursorTransform.localPosition.x, CameraZoom - cursorTransform.localPosition.x),
                0f,
                Mathf.Clamp(CameraZoom * currentCursorAction.y,
                    -CameraZoom - cursorTransform.localPosition.z, CameraZoom - cursorTransform.localPosition.z));

            cursorTransform.localPosition += clampedCursorOffset;

            if (currentPrimaryAction == ActionType.LeftDrag)
            {
                // Left click
                selectionBoxTransform.localScale = new Vector3(Mathf.Abs(clampedCursorOffset.x), 1, Mathf.Abs(clampedCursorOffset.z));
                selectionBoxTransform.localPosition = cursorTransform.localPosition - clampedCursorOffset / 2;

                if (!currentShiftAction)
                {
                    selectedUnits.Clear();
                }

                foreach (Collider col in Physics.OverlapBox(selectionBoxTransform.position,
                             selectionBoxTransform.localScale / 2,
                             Quaternion.identity,
                             interactableLayerMask))
                {
                    if (col.TryGetComponent(out Unit unit) && ownedUnits.Contains(unit) &&
                        !selectedUnits.Contains(unit))
                    {
                        selectedUnits.Add(unit);
                    }
                }
            }
            else if (currentPrimaryAction == ActionType.RightClick)
            {
                //Right click
                Ray ray = new Ray(cursorTransform.position + Vector3.up * 25f, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, interactableLayerMask))
                {
                    CreateAndAssignOrder(hitInfo, selectedUnits, currentShiftAction);
                }
            }
        }
        
        Zoom(currentZoomAction);
    }

    private void Zoom(float currentZoomDelta)
    {
        float clampedDeltaZoom = Mathf.Clamp(currentZoomDelta * zoomSpeed, zoomMinMax.x - CameraZoom, zoomMinMax.y - CameraZoom);

        if (clampedDeltaZoom < 0)
        {
            cameraTransform.localPosition += cursorTransform.localPosition * (1 - Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom + clampedDeltaZoom)) * 0.4f;
        }
            
        cursorTransform.localPosition *= (CameraZoom + clampedDeltaZoom) / CameraZoom;
        CameraZoom += clampedDeltaZoom;
            
        Vector3 mapSizeCorrectedCameraPosition = new Vector3(
            Mathf.Clamp(cameraTransform.localPosition.x, -MapSize + CameraZoom, MapSize - CameraZoom),
            0f,
            Mathf.Clamp(cameraTransform.localPosition.z, -MapSize + CameraZoom, MapSize - CameraZoom));

        cameraTransform.localPosition = mapSizeCorrectedCameraPosition;
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
        
        Order order = Instantiate(orderPrefab, groundHitPosition, Quaternion.identity, transform);

        order.SetOrder(hitInfo.transform, orderType, capableUnits, groundOrder, groundHitPosition, this, additive);
    }
    
    public void UnitCollectedMass(float amount)
    {
        AddReward(amount);
        // Debug.Log(amount);
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
}