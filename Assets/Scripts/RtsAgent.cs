using System.Collections.Generic;
using System.Linq;
using AgentDebugTool.Scripts.Agent;
using Systems.Interfaces;
using Systems.Orders;
using Templates;
using Tools;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Cursor = UnityEngine.Cursor;
using Random = UnityEngine.Random;

public class RtsAgent : DebuggableAgent
{
    public enum AgentActionType
    {
        None = 0,
        LeftDrag = 1,
        RightClick = 2
    }
    
    public class RtsAgentAction
    {
        public Vector2 currentCursorAction;
        public float timeForScheduledDecision;
        public AgentActionType currentAgentActionType;
        public bool currentShiftAction;
        public int currentZoomAction;
        public RtsAgentAction(Vector2 currentCursorAction, float timeForScheduledDecision, AgentActionType currentAgentActionType, bool currentShiftAction, int currentZoomAction)
        {
            this.currentCursorAction = currentCursorAction;
            this.timeForScheduledDecision = timeForScheduledDecision;
            this.currentAgentActionType = currentAgentActionType;
            this.currentShiftAction = currentShiftAction;
            this.currentZoomAction = currentZoomAction;
        }

        public RtsAgentAction()
        {
            
        }
    }
    
    public class RtsAgentObservation
    {
        public float[] vectorObservations;
        public List<List<float[]>> observations;
            
        public RtsAgentObservation(float[] vectorObservations, List<List<float[]>> observations)
        {
            this.vectorObservations = vectorObservations;
            this.observations = observations;
        }
    }

    public class AgentStep
    {
        public RtsAgentAction rtsAgentAction;
        public RtsAgentObservation rtsAgentObservation;
        public float reward;

        public AgentStep(RtsAgentObservation rtsAgentObservation, RtsAgentAction rtsAgentAction, float reward)
        {
            this.rtsAgentObservation = rtsAgentObservation;
            this.rtsAgentAction = rtsAgentAction;
            this.reward = reward;
        }
    }
    
    [Header("Systems")]
    [SerializeField] private BufferSensorComponent reclaimSensorComponent;
    [SerializeField] private BufferSensorComponent unitSensorComponent;
    [SerializeField] private BufferSensorComponent orderSensorComponent;
    [SerializeField] public Environment environment;
    [SerializeField] public Order orderPrefab;
    [SerializeField] private Camera cam;
    
    [Header("Transforms")]
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
    [SerializeField] private float cameraMovementBorderPercentage;
    [SerializeField] private float cameraPanningSpeed;
    [SerializeField] private Vector2 zoomMinMax;
    [SerializeField] private Vector3 minMeanMaxActionDelay;
    [SerializeField] private LayerMask interactableLayerMask;

    [Header("Extra")]
    [SerializeField] private bool isHuman;
    [SerializeField] private bool drawBufferSensorMonitor;
    
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

    private float MapSize => environment.halfGroundSize;
    private Vector3 CursorLocalPosition => cursorTransform.localPosition;
    private Vector3 CameraLocalPosition => cameraTransform.localPosition;
    
    private readonly List<Unit> ownedUnits = new List<Unit>();
    private readonly List<Unit> selectedUnits = new List<Unit>();
    
    private RtsAgentAction completedRtsAgentAction;
    private float timeSinceLastDecision;
    private Vector3 accumulatedCameraOffset;
    private readonly List<AgentStep> episodeTrajectory = new List<AgentStep>();
    
    public override void Initialize()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
            
    }
    
    public void Start()
    {
        Academy.Instance.EnvironmentStep();
    }

    public override void OnEpisodeBegin()
    {
        CameraZoom = zoomMinMax.x;
        cameraTransform.localPosition = Vector3.zero;
        cursorTransform.localPosition = Vector3.zero;
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;
        
        ownedUnits.Clear();
        selectedUnits.Clear();

        completedRtsAgentAction = null;
        timeSinceLastDecision = 0;
        accumulatedCameraOffset = Vector3.zero;

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

    private void SpawnUnit(UnitTemplate unitTemplate, Vector3 localPosition)
    {
        Unit unit = ObjectPooler.InstantiateGameObject(unitPrefab, transform.parent.localPosition + localPosition, Quaternion.identity, transform.parent);
        unit.SetUnitTemplate(unitTemplate, this);
        unit.OnDestroyableDestroy += HandleUnitDestroyed;
        ownedUnits.Add(unit);
    }
    
    public void FixedUpdate()
    {
        timeSinceLastDecision += Time.fixedDeltaTime;
            
        if (isHuman)
        {
            if (timeSinceLastDecision >= minMeanMaxActionDelay.z || completedRtsAgentAction.timeForScheduledDecision != 0)
            {
                completedRtsAgentAction.timeForScheduledDecision = timeSinceLastDecision;
                episodeTrajectory.Add(new AgentStep(GetAgentEnvironmentObservation(), completedRtsAgentAction, 0));
                timeSinceLastDecision = 0;
                completedRtsAgentAction = new RtsAgentAction(Vector2.zero, 0, AgentActionType.None, false, 0);
                InteractWithEnvironment(completedRtsAgentAction);
            }
        }
        else if (!isHuman)
        {
            if (timeSinceLastDecision >= completedRtsAgentAction.timeForScheduledDecision)
            {
                timeSinceLastDecision = 0;
                RequestDecision();
                Academy.Instance.EnvironmentStep();
                InteractWithEnvironment(completedRtsAgentAction);
            }
        }
    }
    
    private void Update()
    {
        if (!isHuman || 
            !Application.isFocused ||
            completedRtsAgentAction != null ||
            IsMouseOutOfScreen()) return;

        timeSinceLastDecision += Time.deltaTime;
        
        Vector3 humanCursorPositionRelativeToCamera = cameraTransform.InverseTransformPoint(cam.ScreenToWorldPoint(Input.mousePosition));
        float startCameraMovementDistance = CameraZoom * (1 - cameraMovementBorderPercentage);

        Vector3 cameraOffsetDirection = Vector3.zero;
        
        if (IsAxisOutOfCameraMovementThreshold(humanCursorPositionRelativeToCamera.x, startCameraMovementDistance))
        {
            cameraOffsetDirection += Vector3.right * Mathf.Sign(humanCursorPositionRelativeToCamera.x);
        }
        
        if (IsAxisOutOfCameraMovementThreshold(humanCursorPositionRelativeToCamera.z, startCameraMovementDistance))
        {
            cameraOffsetDirection += Vector3.forward * Mathf.Sign(humanCursorPositionRelativeToCamera.z);
        }
        
        if (cameraOffsetDirection != Vector3.zero)
        {
            if (accumulatedCameraOffset != Vector3.zero && cameraOffsetDirection.normalized != accumulatedCameraOffset.normalized)
            {
                completedRtsAgentAction ??= new RtsAgentAction
                {
                    currentAgentActionType = AgentActionType.None
                };
            }
            else
            {
                Vector3 cameraOffset = cameraOffsetDirection.normalized * (cameraPanningSpeed * Time.deltaTime * zoomSpeed / zoomMinMax.x);
                Vector3 cameraOffsetCorrected = new Vector3(
                    GetClampedCameraOffset(cameraOffset.x, CameraLocalPosition.x, CursorLocalPosition.x),
                    0f,
                    GetClampedCameraOffset(cameraOffset.z, CameraLocalPosition.z, CursorLocalPosition.z));

                Vector3 correction = cameraOffsetCorrected - cameraOffset;
                
                if (Mathf.Abs(correction.x) > 0 || Mathf.Abs(correction.z) > 0)
                {
                    completedRtsAgentAction ??= new RtsAgentAction
                    {
                        currentAgentActionType = AgentActionType.None
                    };
                }
            
                accumulatedCameraOffset += cameraOffsetCorrected;
                cameraTransform.localPosition += cameraOffsetCorrected;
                cursorTransform.localPosition -= cameraOffsetCorrected;
            }
        }
        else if (accumulatedCameraOffset != Vector3.zero)
        {
            completedRtsAgentAction ??= new RtsAgentAction
            {
                currentAgentActionType = AgentActionType.None
            };
        }
        else if (Input.mouseScrollDelta.y != 0)
        {
            completedRtsAgentAction ??= new RtsAgentAction
            {
                //currentAgentActionType = -Input.mouseScrollDelta.y < 0 ? AgentActionType.ZoomOut : AgentActionType.ZoomIn
            };
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            completedRtsAgentAction ??= new RtsAgentAction
            {
                currentAgentActionType = AgentActionType.None
            };
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            completedRtsAgentAction ??= new RtsAgentAction
            {
                currentAgentActionType = AgentActionType.LeftDrag
            };
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            completedRtsAgentAction ??= new RtsAgentAction
            {
                currentAgentActionType = AgentActionType.RightClick
            };
        }

        if (completedRtsAgentAction != null)
        {
            if (completedRtsAgentAction.currentAgentActionType == AgentActionType.None && 
                accumulatedCameraOffset != Vector3.zero)
            {
                Vector3 totalHumanCursorOffset = accumulatedCameraOffset - CursorLocalPosition;
                completedRtsAgentAction.currentCursorAction = new Vector2(
                    Mathf.Clamp(totalHumanCursorOffset.x / CameraZoom / 2, -1f, 1), 
                    Mathf.Clamp(totalHumanCursorOffset.z / CameraZoom / 2, -1f, 1));
            }
            else
            {
                Vector3 humanCursorPositionRelativeToCursor = cursorTransform.InverseTransformPoint(cam.ScreenToWorldPoint(Input.mousePosition));

                if (accumulatedCameraOffset != Vector3.zero)
                {
                    Debug.LogError($"Accumulated offset with wrong action type: {completedRtsAgentAction.currentAgentActionType.ToString()}");
                }
                
                completedRtsAgentAction.currentCursorAction = new Vector2(
                    Mathf.Clamp(humanCursorPositionRelativeToCursor.x / CameraZoom / 2, -1f, 1), 
                    Mathf.Clamp(humanCursorPositionRelativeToCursor.z / CameraZoom / 2, -1f, 1));
            }
            
            cameraTransform.localPosition -= accumulatedCameraOffset;
            cursorTransform.localPosition += accumulatedCameraOffset;
            //Debug.Log($"after reset cameraTransform.localPosition {cameraTransform.localPosition}, cursorTransform.localPosition {cursorLocalPosition}");
            
            completedRtsAgentAction.timeForScheduledDecision = timeSinceLastDecision;
            timeSinceLastDecision = 0;
            accumulatedCameraOffset = Vector3.zero;
         
            //Debug.Log($"Update end. timeSinceDecision: {timeSinceDecision}, timeWhenToDecide: {timeWhenToDecide}, shouldRequestDecision: {shouldRequestDecision}");
        
            if (Input.GetKey(KeyCode.LeftShift))
            {
                completedRtsAgentAction.currentShiftAction = true;
            }
         
            InteractWithEnvironment(completedRtsAgentAction);
        }
    }

    private float GetClampedCameraOffset(float value, float cameraAxisPosition, float cursorAxisPosition)
    {
        return Mathf.Clamp(value,
            Mathf.Max(-MapSize - cameraAxisPosition + CameraZoom, cursorAxisPosition - CameraZoom),
            Mathf.Min(MapSize - cameraAxisPosition - CameraZoom, cursorAxisPosition + CameraZoom));
    }
    
    private bool IsMouseOutOfScreen()
    {
        return Input.mousePosition.x < 0 ||
               Input.mousePosition.x > 1000 ||
               Input.mousePosition.y < 0 ||
               Input.mousePosition.y > 1000;
    }

    private bool IsAxisOutOfCameraMovementThreshold(float axisPosition, float threshold)
    {
        return axisPosition >= threshold || axisPosition <= -threshold;
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        continuousActions[0] = completedRtsAgentAction.currentCursorAction.x;
        continuousActions[1] = completedRtsAgentAction.currentCursorAction.y;
        continuousActions[2] = Mathf.InverseLerp(minMeanMaxActionDelay.x, minMeanMaxActionDelay.y, completedRtsAgentAction.timeForScheduledDecision) + 1 / 2f;
        
        discreteActions[0] = (int)completedRtsAgentAction.currentAgentActionType;
        discreteActions[1] = completedRtsAgentAction.currentShiftAction ? 1 : 0;

        completedRtsAgentAction = null;
    }

    public RtsAgentObservation GetAgentEnvironmentObservation()
    {
        Monitor.RemoveAllValuesFromAllTransforms();

        Vector3 camRelNormPos = cursorTransform.InverseTransformPoint(cameraTransform.position) / CameraZoom;
        Vector3 cursorPosObservation = (cameraTransform.localPosition + cursorTransform.localPosition) / MapSize;
        float zoomObservation = Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom);
        float time = environment.timeSinceReset / environment.timeWhenReset;
        float timeSinceDecision = timeSinceLastDecision / minMeanMaxActionDelay.z;
        
        float[] vectorObservations =
        {
            camRelNormPos.x,
            camRelNormPos.z,
            cursorPosObservation.x,
            cursorPosObservation.z,
            zoomObservation,
            time,
            timeSinceDecision 
        };
        
        observationsDebugSet.Add("Cam Pos X", $"{camRelNormPos.x}");
        observationsDebugSet.Add("Cam Pos Z", $"{camRelNormPos.z}");
        
        observationsDebugSet.Add("Cursor Pos X", $"{cursorPosObservation.x}");
        observationsDebugSet.Add("Cursor Pos Z", $"{cursorPosObservation.z}");
        
        observationsDebugSet.Add("Zoom", $"{zoomObservation}");
        observationsDebugSet.Add("Time", $"{time}");
        observationsDebugSet.Add("Time Since Decision", $"{timeSinceDecision}");

        List<List<float[]>> observations = new List<List<float[]>>();
        observations.Add(new List<float[]>());
        observations.Add(new List<float[]>());
        observations.Add(new List<float[]>());
        
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
                observations[0].Add(interactableObservation.ToArray());
                
                if (drawBufferSensorMonitor)
                {
                    Monitor.Log("Data: ", string.Join(" ", interactableObservation), collider.transform);
                    Monitor.Log("Type: ", "Reclaim", collider.transform);
                }
            }
            else if (collider.TryGetComponent(out Unit unit))
            {
                interactableObservation.Add(selectedUnits.Contains(unit) ? 1 : -1);
                observations[1].Add(interactableObservation.ToArray());
                
                if (drawBufferSensorMonitor)
                {
                    Monitor.Log("Data: ", string.Join(" ", interactableObservation), collider.transform);
                    Monitor.Log("Type: ", "Unit", collider.transform);
                }
            }
            else if (collider.TryGetComponent(out Order order))
            {
                foreach (Unit assignedUnit in order.assignedUnits)
                {
                    Vector3 relUnitNormPos = cursorTransform.InverseTransformPoint(assignedUnit.transform.position) / MapSize;
                    Vector3 relOrderNormPos = cursorTransform.InverseTransformPoint(order.transform.position) / MapSize;
                
                    List<float> orderObservation = new List<float>
                    {
                        relUnitNormPos.x,
                        relUnitNormPos.z,
                        relOrderNormPos.x,
                        relOrderNormPos.z,
                        order.orderType == OrderType.Move ? 1 : 0,
                        order.orderType == OrderType.Reclaim ? 1 : 0,
                        assignedUnit.assignedOrders.IndexOf(order)
                    };

                    if (drawBufferSensorMonitor)
                    {
                        Monitor.Log("Data: ", string.Join(" ", orderObservation), order.transform);
                        Monitor.Log("Type: ", "Order", order.transform);
                    }

                    observations[2].Add(orderObservation.ToArray());
                }
            }
        }
        //Debug.Log($"Obs. environment.timeSinceReset at {environment.timeSinceReset}, agent time since decision {timeSinceDecision}");
        BroadcastObservationsCollected();
        
        return new RtsAgentObservation(vectorObservations, observations);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        RtsAgentObservation rtsAgentObservation = isHuman ? episodeTrajectory[0].rtsAgentObservation : GetAgentEnvironmentObservation();
            
        foreach (float observation in rtsAgentObservation.vectorObservations)
        {
            sensor.AddObservation(observation);
        }

        foreach (float[] objectObservation in rtsAgentObservation.observations[0])     
        {
            reclaimSensorComponent.AppendObservation(objectObservation);
        }
        
        foreach (float[] objectObservation in rtsAgentObservation.observations[1])     
        {
            unitSensorComponent.AppendObservation(objectObservation);
        }
        
        foreach (float[] objectObservation in rtsAgentObservation.observations[2])     
        {
            orderSensorComponent.AppendObservation(objectObservation);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        Vector2 currentCursorAction = new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));
        float currentDelayAction = Mathf.Clamp(continuousActions[2], -1f, 1f);

        AgentActionType currentAgentActionType = (AgentActionType)discreteActions[0];
        bool currentShiftAction = discreteActions[1] == 1;

        int currentZoomAction = discreteActions[2] - 1;

        float pos = cursorTransform.localPosition.magnitude / CameraZoom;
        float posCam = cameraTransform.localPosition.magnitude / CameraZoom;

        // Regularize continuous actions
        AddReward(-0.005f);
        AddReward(-0.005f * currentCursorAction.magnitude);
        AddReward(-0.005f * pos);
        AddReward(-0.005f * posCam);
        AddReward(currentZoomAction != 0 ? -0.01f : 0f);
        
        if (!isHuman)
        {
            float timeForScheduledDecision = Mathf.Lerp(minMeanMaxActionDelay.x, minMeanMaxActionDelay.y, (currentDelayAction + 1 ) / 2);
            completedRtsAgentAction = new RtsAgentAction(currentCursorAction, timeForScheduledDecision, currentAgentActionType, currentShiftAction, currentZoomAction);
        }
        //Debug.Log($"Action. currentDelayAction: {currentDelayAction}");
    }

    private void InteractWithEnvironment(RtsAgentAction rtsAgentAction)
    {
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;
        
        // Move cursor
        Vector3 desiredCursorOffset = 2 * new Vector3(
            CameraZoom * rtsAgentAction.currentCursorAction.x,
            0,
            CameraZoom * rtsAgentAction.currentCursorAction.y);
        
        Vector3 clampedCursorOffset = new Vector3(
            Mathf.Clamp(desiredCursorOffset.x, -CameraZoom - cursorTransform.localPosition.x, CameraZoom - cursorTransform.localPosition.x),
            0f,
            Mathf.Clamp(desiredCursorOffset.z, -CameraZoom - cursorTransform.localPosition.z, CameraZoom - cursorTransform.localPosition.z));
        
        Vector3 cursorOffsetCorrection = clampedCursorOffset - desiredCursorOffset;
        cursorTransform.localPosition += clampedCursorOffset;
        
        //Zoom camera and correct mouse
        
        if (rtsAgentAction.currentZoomAction != 0)
        {
            float clampedZoomOffset = Mathf.Clamp(rtsAgentAction.currentZoomAction * zoomSpeed, zoomMinMax.x - CameraZoom, zoomMinMax.y - CameraZoom);

            if (clampedZoomOffset < 0)
            {
                cameraTransform.localPosition += cursorTransform.localPosition * ((1 - Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom + clampedZoomOffset)) * 0.4f);
            }
            
            cursorTransform.localPosition *= (CameraZoom + clampedZoomOffset) / CameraZoom;
            CameraZoom += clampedZoomOffset;
        }
        
        // Move Camera
        Vector3 desiredCameraOffset = new Vector3(
            desiredCursorOffset.x > 0 ? Mathf.Min(cursorOffsetCorrection.x, 0) : Mathf.Max(cursorOffsetCorrection.x, 0),
            0,
            desiredCursorOffset.z > 0 ? Mathf.Min(cursorOffsetCorrection.z, 0) : Mathf.Max(cursorOffsetCorrection.z, 0));

        Vector3 desiredCameraPosition = cameraTransform.localPosition - desiredCameraOffset;
            
        cameraTransform.localPosition = new Vector3(
            Mathf.Clamp(desiredCameraPosition.x, -MapSize + CameraZoom, MapSize - CameraZoom),
            0f,
            Mathf.Clamp(desiredCameraPosition.z, -MapSize + CameraZoom, MapSize - CameraZoom));
        
        if (rtsAgentAction.currentAgentActionType == AgentActionType.LeftDrag)
        {
            // Left click
            selectionBoxTransform.localScale = new Vector3(Mathf.Abs(clampedCursorOffset.x), 1, Mathf.Abs(clampedCursorOffset.z));
            selectionBoxTransform.localPosition = cursorTransform.localPosition - clampedCursorOffset / 2;

            if (!rtsAgentAction.currentShiftAction)
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
        else if (rtsAgentAction.currentAgentActionType == AgentActionType.RightClick)
        {
            //Right click
            Ray ray = new Ray(cursorTransform.position + Vector3.up * 25f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, interactableLayerMask))
            {
                CreateAndAssignOrder(hitInfo, selectedUnits, rtsAgentAction.currentShiftAction);
            }
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

    public void UnAssignedUnitOrder()
    {
        //AddReward(-0.005f);
    }
}