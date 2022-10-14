using System.Collections.Generic;
using System.Linq;
using Systems.Orders;
using Templates;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class RtsAgent : Agent
{
    private enum ActionType
    {
        None = 0,
        MoveCursor = 1,
        MoveCamera = 2,
        Zoom = 3,
        LeftDrag = 4,
        RightClick = 5
    }

    [Header("Objects")]
    [SerializeField] private BufferSensorComponent reclaimSensorComponent;
    [SerializeField] private BufferSensorComponent unitSensorComponent;
    [SerializeField] private BufferSensorComponent orderSensorComponent;
    [SerializeField] private BufferSensorComponent linkSensorComponent;
    [SerializeField] private Environment environment;
    [SerializeField] public OrderManager orderManager;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraGizmoTransform;
    [SerializeField] private Transform cursorTransform;
    [SerializeField] private Transform selectionBoxTransform;
    
    [Header("Unit")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private UnitTemplate startingUnitTemplate;
    [SerializeField] private int numStartingUnits = 1;
    
    [Header("Parameters")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Vector2 zoomMinMax;
    [SerializeField] private bool writeHeuristic;
    [SerializeField] private LayerMask interactableLayerMask;

    public readonly List<Unit> ownedUnits = new List<Unit>();
    public readonly List<Unit> selectedUnits = new List<Unit>();
    
    private Vector2 cursorAction;
    private ActionType actionType = ActionType.None;
    private bool shiftAction;

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
        CameraZoom = Random.Range(zoomMinMax.x, zoomMinMax.y);
        cameraTransform.localPosition = Vector3.zero;
        cursorTransform.localPosition = Vector3.zero;
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;
        
        ownedUnits.Clear();
        selectedUnits.Clear();
        
        cursorAction = Vector2.zero;
        actionType = ActionType.None;
        shiftAction = false;

        SpawnStartingUnits();
    }
    
    private void SpawnStartingUnits()
    {
        for (int i = 0; i < numStartingUnits; i++)
        {
            Vector3 localPosition = new Vector3(
                Random.Range(-MapSize, MapSize), 
                0, 
                Random.Range(-MapSize, MapSize));
            
            Unit unit = Instantiate(unitPrefab, transform.parent.localPosition + localPosition, Quaternion.identity, transform.parent);
            unit.SetUnitTemplate(startingUnitTemplate, this, environment);
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(cameraTransform.localPosition.x / MapSize);
        sensor.AddObservation(cameraTransform.localPosition.z / MapSize);

        float zoomObservation = Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, CameraZoom) * 2 - 1;
        sensor.AddObservation(zoomObservation);

        bool debugged = false;
        
        foreach (Collider collider in Physics.OverlapBox(cam.transform.position, cam.transform.localScale / 2, Quaternion.identity, interactableLayerMask))
        {
            List<float> interactableObservation = new List<float>
            {
                (collider.transform.localPosition.x - cameraTransform.localPosition.x) / CameraZoom, 
                (collider.transform.localPosition.z - cameraTransform.localPosition.z) / CameraZoom,
            };

            
            // if (collider.TryGetComponent(out Reclaim reclaim))
            // {
            //     interactableObservation.Add(reclaim.Amount);
            //     reclaimSensorComponent.AppendObservation(interactableObservation.ToArray());
            // }
            // else if (collider.TryGetComponent(out Unit unit))
            // {
            //     interactableObservation.Add(ownedUnits.Contains(unit) ? 1 : 0);
            //     unitSensorComponent.AppendObservation(interactableObservation.ToArray());
            // }
            // else if (collider.TryGetComponent(out Order order))
            // {
            //     interactableObservation.Add((int)order.OrderData.orderType);
            //     orderSensorComponent.AppendObservation(interactableObservation.ToArray());
            // }

                
            // if (!debugged)
            // {
            //     //Debug.Log($"Obs: target: {obs[0]}, {obs[1]}, cam zoom: {zoomObservation}");
            //     debugged = true;
            // }
        }

        
        // TODO: Figure out this monstrosity
        
        foreach (Unit ownedUnit in ownedUnits)
        {
            //transitionSensorComponent.AppendObservation();
        }
        
        // foreach (Transition transition in orderManager.GetTransitions())
        // {
        //     foreach (Unit transitionAssignedUnit in transition.assignedUnits)
        //     {
        //         List<float> transitionObservation = new List<float>();
        //         //transitionObservation.Add();
        //         //transitionSensorComponent.AppendObservation();
        //     }
        // }
    }

    private void Update()
    {
        if (!writeHeuristic) return;
        
        if (Input.mouseScrollDelta.y != 0 || actionType == ActionType.Zoom)
        {
            cursorAction = new Vector2(0, Mathf.Clamp(cursorAction.y - Input.mouseScrollDelta.y / 3f, -1f, 1f));
            actionType = ActionType.Zoom;
            return;
        }

        if (actionType != ActionType.None) return;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            actionType = ActionType.LeftDrag;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            actionType = ActionType.RightClick;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            actionType = ActionType.MoveCursor;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            actionType = ActionType.MoveCamera;
        }
        else
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftAction = true;
        }
                
        Vector3 cursorLocalPos = cursorTransform.transform.InverseTransformPoint(cam.ScreenToWorldPoint(new Vector3(
            Mathf.Clamp(Input.mousePosition.x, 0, 1000), 
            Mathf.Clamp(Input.mousePosition.y, 0, 1000),
            25)));
        
        cursorAction = new Vector2(
            Mathf.Clamp(cursorLocalPos.x / CameraZoom / 2, -1f, 1),
            Mathf.Clamp(cursorLocalPos.z / CameraZoom / 2, -1f, 1));
        
        //Debug.Log($"Heuristic action: {cursorLocalPos.x / CameraZoom / 2}, {cursorAction}, {actionType}, {shiftAction}");
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        discreteActions[0] = actionType == ActionType.None ? (int)ActionType.MoveCursor : (int)actionType;
        discreteActions[1] = shiftAction ? 1 : 0;
        
        continuousActions[0] = cursorAction.x;
        continuousActions[1] = cursorAction.y;
        
        cursorAction = Vector2.zero;
        actionType = ActionType.None;
        shiftAction = false;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        selectionBoxTransform.localScale = Vector3.zero;
        selectionBoxTransform.localPosition = Vector3.zero;

        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        Vector2 continuousAction = new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));

        ActionType currentActionType = (ActionType)discreteActions[0];
        bool currentShiftAction = discreteActions[1] == 1;
        
        AddReward(-0.01f);
        AddReward(-0.1f * continuousAction.y * continuousAction.y);
        
        if (currentActionType == ActionType.Zoom)
        {
            float clampedDeltaZoom = Mathf.Clamp(continuousAction.y * zoomSpeed, zoomMinMax.x - CameraZoom, zoomMinMax.y - CameraZoom);
            
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
            
            return;
        }

        AddReward(-0.1f * continuousAction.x * continuousAction.x);

        if (currentActionType == ActionType.MoveCamera)
        {
            Vector3 desiredCameraLocalPosition = cameraTransform.localPosition + new Vector3(
                CameraZoom * 2 * continuousAction.x,
                0f,
                CameraZoom * 2 * continuousAction.y);
            
            cameraTransform.localPosition = new Vector3(
                Mathf.Clamp(desiredCameraLocalPosition.x, -MapSize + CameraZoom, MapSize - CameraZoom),
                0f,
                Mathf.Clamp(desiredCameraLocalPosition.z, -MapSize + CameraZoom, MapSize - CameraZoom));
            
            return;
        }
        
        Vector3 clampedCursorLocalPosition = new Vector3(
            Mathf.Clamp(cursorTransform.localPosition.x + CameraZoom * 2 * continuousAction.x, -CameraZoom, CameraZoom),
            0f,
            Mathf.Clamp(cursorTransform.localPosition.z + CameraZoom * 2 * continuousAction.y, -CameraZoom, CameraZoom));
        
        if (currentActionType == ActionType.LeftDrag)
        {
            Vector3 cursorOffset = clampedCursorLocalPosition - cursorTransform.localPosition;
            
            selectionBoxTransform.localScale = new Vector3(Mathf.Abs(cursorOffset.x), 1, Mathf.Abs(cursorOffset.z));
            selectionBoxTransform.localPosition = cursorTransform.localPosition + cursorOffset / 2;
            
            if (!currentShiftAction)
            {
                selectedUnits.Clear();
            }
            
            foreach (Collider col in Physics.OverlapBox(selectionBoxTransform.position, 
                         selectionBoxTransform.localScale / 2, 
                         Quaternion.identity, 
                         interactableLayerMask))  
            {
                if (col.TryGetComponent(out Unit unit) && ownedUnits.Contains(unit) && !selectedUnits.Contains(unit))
                {
                    selectedUnits.Add(unit);
                }
            }
        }

        cursorTransform.localPosition = clampedCursorLocalPosition;

        if (currentActionType != ActionType.RightClick) return;
        
        Ray ray = new Ray(cursorTransform.position + Vector3.up * 25f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, interactableLayerMask))
        {
            orderManager.IssueOrderToSelectedUnits(hitInfo, selectedUnits, currentShiftAction);
        }
        //Debug.Log($"Agent action is: {continuousAction}, {discreteActions[0]}");
    }
    
    public void OrderComplete(OrderData orderData, Unit unit)
    {
        AddReward(0.01f);
        //Debug.Log("Order completed");
    }
    
    public void UnitCollectedMass(float amount)
    {
        AddReward(amount);
        //Debug.Log($"Reclaimed {amount} mass");
    }
}