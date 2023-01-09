using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Agents;
using Tools;
using Unity.MLAgents;
using UnityEngine;

public class AIPlayer : Player
{
    public AIPlayerObservation currentObservation;
    
    [SerializeField] private UnitSelectorAgent unitSelectorAgent;
    [SerializeField] private PositionSelectorAgent positionSelectorAgent;
    [SerializeField] private int numOrderObservationsPerUnit;
    [SerializeField] private bool drawBufferSensorMonitor;
    [SerializeField] private float timeBetweenDecision;
    
    private float timeSinceDecision;

    public override void Reset()
    {
        base.Reset();
        timeSinceDecision = 0;
        unitSelectorAgent.EndEpisode();
        positionSelectorAgent.EndEpisode();
    }

    public override void AddReward(float amount)
    {
        base.AddReward(amount);
        unitSelectorAgent.AddReward(amount);
        positionSelectorAgent.AddReward(amount);
    }

    public override void UnitCollectedMass(float amount)
    {
        base.UnitCollectedMass(amount);
        unitSelectorAgent.AddReward(amount / 10);
        positionSelectorAgent.AddReward(amount / 10);
    }

    private void GetAgentEnvironmentObservation()
    {
        Monitor.RemoveAllValuesFromAllTransforms();
        
        float time = environment.timeSinceReset / environment.timeWhenReset;

        float[] vectorObservations =
        {
            time
        };


        List<List<float[]>> observations = new List<List<float[]>>
        {
            new List<float[]>(),
            new List<float[]>()
        };
        
        List<List<float[]>> positionSelectorObservation = new List<List<float[]>>
        {
            new List<float[]>(),
            new List<float[]>()
        };

        var colliders = Physics.OverlapBox(environment.transform.position, Vector3.one * environment.halfGroundSize, Quaternion.identity, interactableLayerMask);
        
        foreach (Collider col in colliders)
        {
            Vector3 relNormPos = environment.transform.InverseTransformPoint(col.transform.position) / environment.halfGroundSize;
            
            List<float> interactableObservation = new List<float>
            {
                relNormPos.x, 
                relNormPos.z,
            };
            
            if (col.TryGetComponent(out Reclaim reclaim))
            {
                interactableObservation.Add(reclaim.Amount / 10f);
                observations[0].Add(interactableObservation.ToArray());
                positionSelectorObservation[0].Add(interactableObservation.ToArray());
                
                if (drawBufferSensorMonitor)
                {
                    Monitor.Log("Data: ", string.Join(" ", interactableObservation.ConvertAll(x => x == 0 || x == 1 ? x.ToString() : x.ToString("F1"))), col.transform);
                    Monitor.Log("Type: ", "Reclaim", col.transform);
                }
            }
            else if (col.TryGetComponent(out Unit unit))
            {
                interactableObservation.AddRange(ConvertIndexToOneHot(ownedUnits.IndexOf(unit), 10));
                interactableObservation.Add(unit.assignedOrders.Count / 5f);
                
                for (int i = 0; i < numOrderObservationsPerUnit; i++)
                {
                    List<float> orderObservation;
                    
                    if (i < unit.assignedOrders.Count)
                    {
                        Vector3 relOrderNormPos = environment.transform.InverseTransformPoint(unit.assignedOrders[i].transform.position) / environment.halfGroundSize;
                        
                        orderObservation = new List<float>
                        {
                            relOrderNormPos.x,
                            relOrderNormPos.z,
                            1
                        };
                    }
                    else
                    {
                        orderObservation = new List<float>
                        {
                            0,
                            0,
                            0
                        };
                    }
                    
                    interactableObservation.AddRange(orderObservation);
                }
                
                if (drawBufferSensorMonitor)
                {
                    Monitor.Log("Data: ", string.Join(" ", interactableObservation.ConvertAll(x => x == 0 || x == 1 ? x.ToString() : x.ToString("F1"))), col.transform);
                    Monitor.Log("Type: ", "Unit", col.transform);
                }

                observations[1].Add(interactableObservation.ToArray());
                interactableObservation.Add(0f);
                positionSelectorObservation[1].Add(interactableObservation.ToArray());
            }
        }
        
        currentObservation = new AIPlayerObservation(
            new UnitSelectorAgentObservation(vectorObservations, observations), 
            new PositionSelectorAgentObservation(vectorObservations, positionSelectorObservation));
    }

    public void FixedUpdate()
    {
        timeSinceDecision += Time.fixedDeltaTime;
        
        if (timeSinceDecision >= timeBetweenDecision)
        {
            timeSinceDecision = 0;
            GetAgentEnvironmentObservation();
            unitSelectorAgent.RequestDecision();
            Academy.Instance.EnvironmentStep();
            positionSelectorAgent.RequestDecision();
            Academy.Instance.EnvironmentStep();
        }
    }
    
    private float[] ConvertIndexToOneHot(int index, int indexCount)
    {
        float[] result = new float[indexCount];
        result[index] = 1;
        
        return result;
    }
}
