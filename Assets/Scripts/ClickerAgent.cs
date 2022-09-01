using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ClickerAgent : Agent
{
    [SerializeField] private BufferSensorComponent bufferSensorComponent;
    [SerializeField] private Transform virtualMouse;
    [SerializeField] private float halfGroundSize;

    [SerializeField] private Camera _camera;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private Vector2 cameraMinMaxZoomSize;
    
    [SerializeField] private Target targetPrefab;
    [SerializeField] private int numTargets;
    
    private readonly List<Target> targets = new List<Target>();
    
    private void Awake()
    {
        for (int i = 0; i < numTargets; i++)
        {
            Target t = Instantiate(targetPrefab, transform.parent);
            t.Initialize(halfGroundSize);
            targets.Add(t);
        }
    }

    public override void OnEpisodeBegin()
    {
        _camera.orthographicSize = 40;
        virtualMouse.localPosition = Vector3.zero;
        transform.localPosition = Vector3.up * 25f;

        foreach (Target target in targets)
        {
            target.Reset();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x / halfGroundSize);
        sensor.AddObservation(transform.localPosition.z / halfGroundSize);
        sensor.AddObservation(_camera.orthographicSize / cameraMinMaxZoomSize.y);

        foreach (Target target in targets)
        {
            if (!target.IsClicked())
            {
                float[] obs = new[] {target.transform.localPosition.x / halfGroundSize, target.transform.localPosition.z / halfGroundSize};
                bufferSensorComponent.AppendObservation(obs);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        Vector2 mouseClickCameraSpacePosition = new Vector2(
            Mathf.Clamp(continuousActions[0], -1f, 1f),
            Mathf.Clamp(continuousActions[1], -1f, 1f));

        virtualMouse.localPosition = new Vector3(_camera.orthographicSize * mouseClickCameraSpacePosition.x, 0, _camera.orthographicSize * mouseClickCameraSpacePosition.y);
        
        Ray ray = new Ray(virtualMouse.position, Vector3.down * 50f);
        if (Physics.Raycast(ray, out RaycastHit hitInfo) && hitInfo.collider.CompareTag("Target"))
        {
            hitInfo.collider.GetComponent<Target>().Clicked();
            AddReward(5f);

            if (targets.All(x => x.IsClicked()))
            {
                AddReward(20f);
                EndEpisode();
            }
        }
        AddReward(-0.05f);

        if (discreteActions[0] != 4)
        {
            Vector3 cameraDeltaPosition = Quaternion.Euler(0, 90f * discreteActions[0], 0) * Vector3.forward * cameraMoveSpeed;
            
            transform.localPosition = new Vector3(
                Mathf.Clamp(transform.localPosition.x + cameraDeltaPosition.x, -halfGroundSize, halfGroundSize),
                25,
                Mathf.Clamp(transform.localPosition.z + cameraDeltaPosition.z, -halfGroundSize, halfGroundSize));
            
            AddReward(-0.01f);
        }

        if (discreteActions[1] != 2)
        {
            float deltaZoom = (discreteActions[1] * 2 - 1) * cameraZoomSpeed;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize + deltaZoom, cameraMinMaxZoomSize.x, cameraMinMaxZoomSize.y);
            AddReward(-0.01f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = (Mathf.Clamp(Input.mousePosition.x, 0, 500) - 250f) / 250f;
        continuousActions[1] = (Mathf.Clamp(Input.mousePosition.y, 0, 500) - 250f) / 250f;

        
        if (Input.GetKey(KeyCode.Alpha1))
        {
            discreteActions[1] = 0;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            discreteActions[1] = 1;
        }
        else
        {
            discreteActions[1] = 2;
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 0;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 2;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 3;
        }
        else
        {
            discreteActions[0] = 4;
        }
    }
}
