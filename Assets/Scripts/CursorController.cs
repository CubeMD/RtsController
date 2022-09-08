using System;
using System.Collections.Generic;
using System.Linq;
using MLAgentsDebugTool.Duplicator;
using Unity.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class CursorController : Agent
{
    [SerializeField] private BufferSensorComponent bufferSensorComponent;
    [SerializeField] private EnvironmentController environmentController;
    
    [SerializeField] private Transform cameraGizmo;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Vector2 zoomMinMax;
    
    [SerializeField] private bool writeHeuristic;

    private Camera cam;
    private float currentZoom;
    
    private float heuristicScrollDelta;
    private Vector2 heuristicCursorPosActionDelta;
    private HeuristicActionType currentHeuristicAction = HeuristicActionType.None;
    
    private enum HeuristicActionType
    {
        None,
        Move,
        Zoom,
        Click,
        Drag,
        RightClick
    }

    public override void Initialize()
    {
        cam = Camera.main;
        environmentController.Initialize();
    }
    
    private void Update()
    {
        if (writeHeuristic)
        {
            if (Input.mouseScrollDelta.y != 0 || currentHeuristicAction == HeuristicActionType.Zoom)
            {
                heuristicScrollDelta = Mathf.Clamp(heuristicScrollDelta - Input.mouseScrollDelta.y / 3f, -1f, 1f);
                currentHeuristicAction = HeuristicActionType.Zoom;
                return;
            }
            
            if (currentHeuristicAction == HeuristicActionType.None)
            {
                Vector3 cursorWorldPos = cam.ScreenToWorldPoint(new Vector3(
                    Mathf.Clamp(Input.mousePosition.x, 0, 500), 
                    Mathf.Clamp(Input.mousePosition.y, 0, 500),
                    25));

                heuristicCursorPosActionDelta = new Vector2(
                    Mathf.Clamp((cursorWorldPos.x - transform.position.x) / currentZoom, -1f, 1),
                    Mathf.Clamp((cursorWorldPos.z - transform.position.z) / currentZoom, -1f, 1));
                
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    currentHeuristicAction = HeuristicActionType.Click;
                }
                else if (Input.GetKey(KeyCode.Mouse1))
                {
                    currentHeuristicAction = HeuristicActionType.RightClick;
                }
                else if (Input.GetKey(KeyCode.Space))
                {
                    currentHeuristicAction = HeuristicActionType.Drag;
                }
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log("Episode began on agent");
        currentZoom = Random.Range(zoomMinMax.x, zoomMinMax.y);
        cameraGizmo.localScale = new Vector3(currentZoom * 2, 1, currentZoom * 2);
        transform.localPosition = Vector3.zero;
        environmentController.Reset();

        currentHeuristicAction = HeuristicActionType.None;
        heuristicScrollDelta = 0;
        heuristicCursorPosActionDelta = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)StepCount / MaxStep);
        
        sensor.AddObservation(transform.localPosition.x / environmentController.halfGroundSize);
        sensor.AddObservation(transform.localPosition.z / environmentController.halfGroundSize);

        float zoomObservation = Mathf.InverseLerp(zoomMinMax.x, zoomMinMax.y, currentZoom) * 2 - 1;
        sensor.AddObservation(zoomObservation);

        bool debugged = false;
        
        foreach (Target target in environmentController.targets)
        {
            if (!target.IsClicked())
            {
                float[] obs = new[] {
                    (target.transform.localPosition.x - transform.localPosition.x) / currentZoom, 
                    (target.transform.localPosition.z - transform.localPosition.z) / currentZoom,
                    target.GetReward()
                };
                
                if (!debugged)
                {
                    //Debug.Log($"Obs: target: {obs[0]}, {obs[1]}, cam zoom: {zoomObservation}");
                    debugged = true;
                }
                
                bufferSensorComponent.AppendObservation(obs);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        Vector2 continuousAction = new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));
        
        if (discreteActions[0] == 1) // zoom
        {
            currentZoom = Mathf.Clamp(currentZoom + continuousAction.y * zoomSpeed, zoomMinMax.x, zoomMinMax.y);
            cameraGizmo.localScale = new Vector3(currentZoom * 2, 1, currentZoom * 2);
            AddReward(-0.1f - 0.1f * continuousAction.y * continuousAction.y);
            return;
        }

        if (discreteActions[0] == 3) //drag
        {
            Vector3 originPos = transform.position;
            
            transform.localPosition = new Vector3(
                Mathf.Clamp(transform.localPosition.x + currentZoom * continuousAction.x, -environmentController.halfGroundSize, environmentController.halfGroundSize), 
                0, 
                Mathf.Clamp(transform.localPosition.z + currentZoom * continuousAction.y, -environmentController.halfGroundSize, environmentController.halfGroundSize));

            Vector3 offset = transform.position - originPos;

            foreach (Collider collider in Physics.OverlapBox(originPos + offset / 2, Abs(offset) / 2))  
            {
                if (collider.CompareTag("Target"))
                {
                    CollectTarget(collider);
                }
            }
            
            return;
        }
        
        transform.localPosition = new Vector3( // move
            Mathf.Clamp(transform.localPosition.x + currentZoom * continuousAction.x, -environmentController.halfGroundSize, environmentController.halfGroundSize), 
            0, 
            Mathf.Clamp(transform.localPosition.z + currentZoom * continuousAction.y, -environmentController.halfGroundSize, environmentController.halfGroundSize));

        if (discreteActions[0] == 2) // click
        {
            Ray ray = new Ray(transform.position + Vector3.up * 25f, Vector3.down * 50f);
            if (Physics.Raycast(ray, out RaycastHit hitInfo) && hitInfo.collider.CompareTag("Target"))
            {
                CollectTarget(hitInfo.collider);
            }
            else
            {
                AddReward(-0.1f);
            }
        }
        
        
        //Debug.Log($"Agent action is: {continuousAction}, {discreteActions[0]}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        discreteActions[0] = currentHeuristicAction == HeuristicActionType.None ? (int)HeuristicActionType.Move - 1 : (int)currentHeuristicAction - 1;
        
        if (currentHeuristicAction == HeuristicActionType.Zoom)
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
        currentHeuristicAction = HeuristicActionType.None;
        
        //Debug.Log($"Heuristic action: {continuousActions[0]}, {continuousActions[1]}, {discreteActions[0]}");
    }

    private void CollectTarget(Collider col)
    {
        AddReward(col.GetComponent<Target>().Collect());

        if (environmentController.targets.All(x => x.IsClicked()))
        {
            AddReward((1f - (float)StepCount / MaxStep) * 100f);
            EndEpisode();
        }
    }
    
    private static Vector3 Abs(Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }
}