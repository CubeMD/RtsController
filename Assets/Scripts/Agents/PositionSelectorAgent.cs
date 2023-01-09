using System.Linq;
using AgentDebugTool.Scripts.Agent;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
    public class PositionSelectorAgent : DebuggableAgent
    {
        [SerializeField] private AIPlayer player;
        [SerializeField] private BufferSensorComponent reclaimSensorComponent;
        [SerializeField] private BufferSensorComponent unitSensorComponent;

        public override void CollectObservations(VectorSensor sensor)
        {
            AIPlayerObservation aiPlayerObservation = player.currentObservation;

            observationsDebugSet.Add("Time", $"{aiPlayerObservation.positionSelectorAgentObservation.vectorObservations[0]:.0}");
            
            foreach (float observation in aiPlayerObservation.positionSelectorAgentObservation.vectorObservations)
            {
                sensor.AddObservation(observation);
            }

            foreach (float[] objectObservation in aiPlayerObservation.positionSelectorAgentObservation.observations[0])
            {
                reclaimSensorComponent.AppendObservation(objectObservation);
            }

            foreach (float[] objectObservation in aiPlayerObservation.positionSelectorAgentObservation.observations[1])
            {
                unitSensorComponent.AppendObservation(objectObservation);
            }

            BroadcastObservationsCollected();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);

            if (player.selectedUnits.Count <= 0) return;

            ActionSegment<float> continuousActions = actions.ContinuousActions;
            ActionSegment<int> discreteActions = actions.DiscreteActions;

            Vector2 currentCursorAction = new Vector2(
                Mathf.Clamp(continuousActions[0], -1f, 1f),
                Mathf.Clamp(continuousActions[1], -1f, 1f));

            bool currentShiftAction = discreteActions[0] == 1;

            player.cursorTransform.localPosition = new Vector3(
                player.environment.halfGroundSize * currentCursorAction.x,
                0,
                player.environment.halfGroundSize * currentCursorAction.y);

            Reclaim targetReclaim = null;
            float minFoundDistance = float.MaxValue;
            float currentDistance;

            foreach (Reclaim reclaim in player.environment.reclaims)
            {
                currentDistance = Vector3.Distance(reclaim.transform.position, player.cursorTransform.position);

                if (currentDistance < minFoundDistance)
                {
                    minFoundDistance = currentDistance;
                    targetReclaim = reclaim;
                }
            }

            if (targetReclaim != null)
            {
                AddReward(-minFoundDistance / player.environment.halfGroundSize);

                Ray ray = new Ray(targetReclaim.transform.position + Vector3.up * 25f, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, player.interactableLayerMask))
                {
                    float distanceFromTargetReward = 0;

                    foreach (Unit selectedUnit in player.selectedUnits)
                    {
                        if (currentShiftAction)
                        {
                            if (selectedUnit.assignedOrders.Count > 0)
                            {
                                distanceFromTargetReward +=
                                    Vector3.Distance(selectedUnit.assignedOrders.Last().position, targetReclaim.transform.position) 
                                    / player.environment.halfGroundSize;
                            }
                            else
                            {
                                distanceFromTargetReward +=
                                    Vector3.Distance(selectedUnit.transform.position, targetReclaim.transform.position) /
                                    player.environment.halfGroundSize;
                            }
                        }
                        else
                        {
                            distanceFromTargetReward +=
                                Vector3.Distance(selectedUnit.transform.position, targetReclaim.transform.position) /
                                player.environment.halfGroundSize;
                        }
                    }

                    AddReward(-distanceFromTargetReward);
                    player.CreateAndAssignOrder(hitInfo, player.selectedUnits, currentShiftAction);
                }
            }
        }
    }
}