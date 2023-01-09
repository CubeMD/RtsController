using System.Collections.Generic;
using System.Linq;
using AgentDebugTool.Scripts.Agent;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
    public class UnitSelectorAgent : DebuggableAgent
    {
        [SerializeField] private AIPlayer player;
        [SerializeField] private BufferSensorComponent reclaimSensorComponent;
        [SerializeField] private BufferSensorComponent unitSensorComponent;
    
        public override void CollectObservations(VectorSensor sensor)
        {
            AIPlayerObservation aiPlayerObservation = player.currentObservation;
            
            observationsDebugSet.Add("Time", $"{aiPlayerObservation.unitSelectorAgentObservation.vectorObservations[0]:.0}");
            
            foreach (float observation in aiPlayerObservation.unitSelectorAgentObservation.vectorObservations)
            {
                sensor.AddObservation(observation);
            }

            foreach (float[] objectObservation in aiPlayerObservation.unitSelectorAgentObservation.observations[0])     
            {
                reclaimSensorComponent.AppendObservation(objectObservation);
            }
        
            foreach (float[] objectObservation in aiPlayerObservation.unitSelectorAgentObservation.observations[1])     
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
            for (int i = 0; i < unitSensorComponent.MaxNumObservables; i++)
            {
                actionMask.SetActionEnabled(i, 1, i < player.ownedUnits.Count);
            }
        }
    
        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
        
            ActionSegment<int> discreteActions = actions.DiscreteActions;

            player.selectedUnits.Clear();

            for (int i = 0; i < unitSensorComponent.MaxNumObservables; i++)
            {
                if (discreteActions[i] == 1)
                {
                    if (i < player.ownedUnits.Count)
                    {
                        player.selectedUnits.Add(player.ownedUnits[i]);

                        if (player.ownedUnits[i].assignedOrders.Count == 0)
                        {
                            AddReward(1f);
                        }
                        
                        player.currentObservation.positionSelectorAgentObservation.observations[1][i][unitSensorComponent.ObservableSize - 1] = 1;
                    }
                    else
                    {
                        Debug.LogError("Selected nonexistent target object");
                    }
                }
            }
        }
    }
}