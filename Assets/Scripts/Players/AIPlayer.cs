using System.Collections.Generic;
using Agents;
using Systems.StateMachine;
using Units;
using Unity.MLAgents;
using UnityEngine;
using Utilities.Agent;

namespace Players
{
    public class AIPlayer : Player
    {
        public RtsAgentObservation currentObservation;
        public Transform cursorTransform;
        
        [SerializeField] private RtsAgent rtsAgent;
        [SerializeField] private bool drawBufferSensorMonitor;
        [SerializeField] private float timeBetweenDecision;
    
        private float timeSinceDecision;
        public override void ResetPlayer()
        {
            base.ResetPlayer();
            timeSinceDecision = 0;
            rtsAgent.EndEpisode();
        }

        private void GetAgentEnvironmentObservation()
        {
            Monitor.RemoveAllValuesFromAllTransforms();
            
            float[] vectorObservations =
            {
                matchManager.TimerPercentage
            };
            
            List<List<float[]>> observations = new List<List<float[]>>
            {
                new List<float[]>(),
                new List<float[]>()
            };

            Collider[] colliders = Physics.OverlapBox(matchManager.transform.position, Vector3.one * matchManager.HalfGroundSize, Quaternion.identity, matchManager.SpaceLayerMask);
        
            foreach (Collider col in colliders)
            {
                Vector3 relNormPos = matchManager.transform.InverseTransformPoint(col.transform.position) / matchManager.HalfGroundSize;
            
                List<float> interactableObservation = new List<float>
                {
                    relNormPos.x, 
                    relNormPos.z,
                };
            
                if (col.TryGetComponent(out Reclaim reclaim))
                {
                    interactableObservation.Add(reclaim.Amount / 10f);
                    observations[0].Add(interactableObservation.ToArray());

                    if (drawBufferSensorMonitor)
                    {
                        Monitor.Log("Data: ", string.Join(" ", interactableObservation.ConvertAll(x => x == 0 || x == 1 ? x.ToString() : x.ToString("F1"))), col.transform);
                        Monitor.Log("Type: ", "Reclaim", col.transform);
                    }
                }
                else if (col.TryGetComponent(out Unit unit))
                {
                    interactableObservation.Add(unit.StateMachine.QueueAmount / 2f);
                    interactableObservation.Add(unitManager.SelectedUnits.Contains(unit) ? 1 : 0);

                    State current = unit.StateMachine.GetActiveState();
                    State last = unit.StateMachine.LastState;

                    if (current == null || current is EmptyState)
                    {
                        // If there are no current & last states
                        // Adds 2 zero vector observations for current and last states
                        addZeroVectorObservation();
                        addZeroVectorObservation();
                    }
                    else
                    {
                        addStatePositionObservation(current);

                        if (last == null || last is EmptyState || last == current)
                        {
                            // Adds zero vector observation for the last state
                            addZeroVectorObservation();
                        }
                        else
                        {
                            addStatePositionObservation(last);
                        }
                    }

                    void addStatePositionObservation(State state)
                    {
                        Vector3 posObservation = matchManager.transform.InverseTransformPoint(state.GetStatePosition()) / matchManager.HalfGroundSize;
                        interactableObservation.AddRange(new List<float> {posObservation.x, posObservation.z, 1});
                    }

                    void addZeroVectorObservation()
                    {
                        interactableObservation.AddRange(new List<float> {0, 0, 0});
                    }

                    if (drawBufferSensorMonitor)
                    {
                        Monitor.Log("Data: ", string.Join(" ", interactableObservation.ConvertAll(x => x == 0 || x == 1 ? x.ToString() : x.ToString("F1"))), col.transform);
                        Monitor.Log("Type: ", "Unit", col.transform);
                    }

                    observations[1].Add(interactableObservation.ToArray());
                }
            }

            currentObservation = new RtsAgentObservation(vectorObservations, observations);
        }
        
        public void FixedUpdate()
        {
            timeSinceDecision += Time.fixedDeltaTime;
        
            if (timeSinceDecision >= timeBetweenDecision)
            {
                timeSinceDecision = 0;
                GetAgentEnvironmentObservation();
                rtsAgent.RequestDecision();
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
}
