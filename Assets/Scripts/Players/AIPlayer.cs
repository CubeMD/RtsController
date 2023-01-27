using System.Collections.Generic;
using Agents;
using Orders;
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
        
            float time = matchManager.matchTimer.timeLeft / matchManager.matchTimer.duration;

            float[] vectorObservations =
            {
                time
            };


            List<List<float[]>> observations = new List<List<float[]>>
            {
                new List<float[]>(),
                new List<float[]>()
            };
        

            var colliders = Physics.OverlapBox(matchManager.transform.position, Vector3.one * matchManager.halfGroundSize, Quaternion.identity, matchManager.spaceLayerMask);
        
            foreach (Collider col in colliders)
            {
                Vector3 relNormPos = matchManager.transform.InverseTransformPoint(col.transform.position) / matchManager.halfGroundSize;
            
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
                    interactableObservation.Add(unit.assignedOrders.Count / 2f);
                    interactableObservation.Add(unitManager.selectedUnits.Contains(unit) ? 1 : 0);

                    if (unit.assignedOrders.Count > 0)
                    {
                        Vector3 firstOrderPos = matchManager.transform.InverseTransformPoint(unit.assignedOrders[0].Position) / matchManager.halfGroundSize;

                        interactableObservation.AddRange(new List<float> {firstOrderPos.x, firstOrderPos.z, 1});
                    }
                    else
                    {
                        interactableObservation.AddRange(new List<float> {0, 0, 0});
                    }
                    
                    if (unit.assignedOrders.Count > 1)
                    {
                        Vector3 lastOrderPos = matchManager.transform.InverseTransformPoint(unit.assignedOrders[unit.assignedOrders.Count - 1].Position) / matchManager.halfGroundSize;
                            
                        interactableObservation.AddRange(new List<float> {lastOrderPos.x, lastOrderPos.z, 1});
                    }
                    else
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
