using System.Collections.Generic;
using System.Linq;
using Systems.Orders;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class RtsAgent : Agent
{
    [SerializeField] private BufferSensorComponent bufferSensorComponent;
    [SerializeField] private Environment environment;
    [SerializeField] private Order orderPrefab;
    
    [SerializeField] private Transform cameraGizmo;
    [SerializeField] private Transform selectionGizmo;
    
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Vector2 zoomMinMax;
    
    [SerializeField] private bool writeHeuristic;

    [SerializeField] private LayerMask interactableLayerMask;

    public OrderGraph OrderGraph { get; private set; } = new OrderGraph();

    private Camera cam;
    private float currentZoom;
    private readonly List<Unit> selectedUnits = new List<Unit>();
    private float heuristicScrollDelta;
    private Vector2 heuristicCursorPosActionDelta;

    private enum ActionType
    {
        None = -1,
        Move = 0,
        Zoom = 1,
        LeftDrag = 2,
        RightClick = 3
    }

    private enum ShiftActionType
    {
        NoShift = 0,
        Shift = 1
    }
    
    private ActionType currentAction = ActionType.None;
    private ShiftActionType currentShiftAction = ShiftActionType.NoShift;

    public override void Initialize()
    {
        cam = Camera.main;
    }

    public override void OnEpisodeBegin()
    {
        currentZoom = Random.Range(zoomMinMax.x, zoomMinMax.y);
        cameraGizmo.localScale = new Vector3(currentZoom * 2, 1, currentZoom * 2);
        transform.localPosition = Vector3.zero;
        selectionGizmo.localScale = Vector3.zero;
        selectionGizmo.localPosition = Vector3.zero;
        OrderGraph = new OrderGraph();
        
        environment.Reset();
        selectedUnits.Clear();
        
        currentAction = ActionType.None;
        currentShiftAction = ShiftActionType.NoShift;
        heuristicScrollDelta = 0;
        heuristicCursorPosActionDelta = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)StepCount / MaxStep);
        
        sensor.AddObservation(transform.localPosition.x / environment.halfGroundSize);
        sensor.AddObservation(transform.localPosition.z / environment.halfGroundSize);

        float zoomObservation = Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, currentZoom) * 2 - 1;
        sensor.AddObservation(zoomObservation);

        bool debugged = false;
        
        int i = 0;
        foreach (Collider collider in Physics.OverlapBox(cameraGizmo.position, cameraGizmo.localScale / 2, Quaternion.identity, interactableLayerMask))
        {
            float[] obs = {};
            
            if (collider.TryGetComponent(out Reclaim reclaim))
            {
                obs = new []
                {
                    (collider.transform.localPosition.x - transform.localPosition.x) / currentZoom, 
                    (collider.transform.localPosition.z - transform.localPosition.z) / currentZoom,
                    reclaim.Amount,
                    1,
                    i++
                };
            }
            else if (collider.TryGetComponent(out Unit unit))
            {
                bool own = environment.UnitBelongsToAgent(unit, this);
                
                 obs = new []
                {
                    (collider.transform.localPosition.x - transform.localPosition.x) / currentZoom, 
                    (collider.transform.localPosition.z - transform.localPosition.z) / currentZoom,
                    own ? 1 : 0,
                    0,
                    i++
                };
            }
            else if (collider.TryGetComponent(out Order order))
            {
                bool own = environment.UnitBelongsToAgent(unit, this);
                
                obs = new []
                {
                    (collider.transform.localPosition.x - transform.localPosition.x) / currentZoom, 
                    (collider.transform.localPosition.z - transform.localPosition.z) / currentZoom,
                    own ? 1 : 0,
                    0,
                    i++
                };
            }

                
            if (!debugged)
            {
                //Debug.Log($"Obs: target: {obs[0]}, {obs[1]}, cam zoom: {zoomObservation}");
                debugged = true;
            }
                
            //bufferSensorComponent.AppendObservation(obs);
        }
    }

    private void Update()
    {
        if (writeHeuristic)
        {
            if (Input.mouseScrollDelta.y != 0 || currentAction == ActionType.Zoom)
            {
                heuristicScrollDelta = Mathf.Clamp(heuristicScrollDelta - Input.mouseScrollDelta.y / 3f, -1f, 1f);
                currentAction = ActionType.Zoom;
                return;
            }
            
            if (currentAction == ActionType.None)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    currentAction = ActionType.LeftDrag;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    currentAction = ActionType.RightClick;
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentAction = ActionType.Move;
                }
                else
                {
                    return;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentShiftAction = ShiftActionType.Shift;
                }
                
                Vector3 cursorWorldPos = cam.ScreenToWorldPoint(new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0, 1000), 
                    Mathf.Clamp(Input.mousePosition.y, 0, 1000),
                    25));

                heuristicCursorPosActionDelta = new Vector2(
                    Mathf.Clamp((cursorWorldPos.x - transform.position.x) / currentZoom, -1f, 1),
                    Mathf.Clamp((cursorWorldPos.z - transform.position.z) / currentZoom, -1f, 1));
            }
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        discreteActions[0] = currentAction == ActionType.None ? (int)ActionType.Move : (int)currentAction;
        discreteActions[1] = (int)currentShiftAction;
        
        if (currentAction == ActionType.Zoom)
        {
            continuousActions[0] = 0;
            continuousActions[1] = heuristicScrollDelta;
        }
        else
        {
            continuousActions[0] = heuristicCursorPosActionDelta.x;
            continuousActions[1] = heuristicCursorPosActionDelta.y;
        }
        
        heuristicScrollDelta = 0;
        heuristicCursorPosActionDelta = Vector2.zero;
        currentAction = ActionType.None;
        currentShiftAction = ShiftActionType.NoShift;

        //Debug.Log($"Heuristic action: {continuousActions[0]}, {continuousActions[1]}, {discreteActions[0]}");
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        selectionGizmo.localScale = Vector3.zero;
        selectionGizmo.localPosition = Vector3.zero;

        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        Vector2 continuousAction = new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));

        ActionType primaryAction = (ActionType)discreteActions[0];
        ShiftActionType secondaryAction = (ShiftActionType)discreteActions[1];
        
        AddReward(-0.01f - 0.1f * continuousAction.y * continuousAction.y);
        
        if (primaryAction == ActionType.Zoom)
        {
            currentZoom = Mathf.Clamp(currentZoom + continuousAction.y * zoomSpeed, zoomMinMax.x, zoomMinMax.y);
            float doubleZoom = currentZoom * 2;
            cameraGizmo.localScale = new Vector3(doubleZoom, 1, doubleZoom);
            return;
        }

        AddReward(-0.1f * continuousAction.x * continuousAction.x);

        Vector3 restrictedOffset = new Vector3(
            Mathf.Clamp(currentZoom * continuousAction.x, -environment.halfGroundSize - transform.localPosition.x, environment.halfGroundSize - transform.localPosition.x),
            0f,
            Mathf.Clamp(currentZoom * continuousAction.y, -environment.halfGroundSize - transform.localPosition.z, environment.halfGroundSize - transform.localPosition.z));
        
        if (primaryAction == ActionType.LeftDrag)
        {
            selectionGizmo.localScale = new Vector3(Mathf.Abs(restrictedOffset.x), 1, Mathf.Abs(restrictedOffset.z));
            selectionGizmo.position = transform.position - restrictedOffset / 2;
            
            if (secondaryAction == ShiftActionType.NoShift)
            {
                selectedUnits.Clear();
            }
            
            foreach (Collider col in Physics.OverlapBox(transform.position + restrictedOffset / 2, selectionGizmo.localScale / 2, Quaternion.identity, interactableLayerMask))  
            {
                if (col.TryGetComponent(out Unit unit) && environment.UnitBelongsToAgent(unit, this) && !selectedUnits.Contains(unit))
                {
                    selectedUnits.Add(unit);
                }
            }
        }

        transform.localPosition += restrictedOffset;

        if (primaryAction != ActionType.RightClick) return;
        
        Ray ray = new Ray(transform.position + Vector3.up * 25f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, interactableLayerMask))
        {
            bool additive = secondaryAction == ShiftActionType.Shift;
            OrderData orderData;
            
            if (hitInfo.collider.TryGetComponent(out Reclaim reclaim))
            {
                orderData = new OrderData(reclaim.transform, OrderType.Reclaim);
                
            }
            else if (hitInfo.collider.TryGetComponent(out Unit unit) && !environment.UnitBelongsToAgent(unit, this))
            {
                orderData = new OrderData(unit.transform, OrderType.Attack);
            }
            else
            {
                orderData = new PositionOrderData(hitInfo.point, hitInfo.transform, OrderType.Move);
            }
            
            CreateOrderAndAssign(orderData, selectedUnits, additive, hitInfo.point);
        }
        
        //Debug.Log($"Agent action is: {continuousAction}, {discreteActions[0]}");
    }

    public void CreateOrderAndAssign(OrderData orderData, List<Unit> assignedUnits, bool additive, Vector3 position)
    {
        List<Unit> capableUnits = assignedUnits.Where(unit => unit.CanExecuteOrderType(orderData.orderType)).ToList();

        if (capableUnits.Count > 0)
        {
            Order order = Instantiate(orderPrefab, position, Quaternion.identity, transform.parent);

            order.OrderData = orderData;
            order.owner = this;
            
            OrderGraph.AddOrder(order);

            foreach (Unit unit in capableUnits)
            {
                if (!additive)
                {
                    OrderGraph.RemoveUnitFromAllTransitions(unit);
                    unit.UnAssignActiveOrder();
                }
            
                if (unit.activeOrder != null)
                {
                    OrderGraph.AddUnitToTransitionOrCreateNew(OrderGraph.LastOrderForUnit(unit), order, unit);
                }
                else
                {
                    unit.TryAssignActiveOrder(order);
                }
            }
        }
    }

    public void OrderComplete()
    {
        AddReward(0.01f);
    }
    
    public void UnitCollectedMass(float amount)
    {
        AddReward(amount);
    }
}