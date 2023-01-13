using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Orders;
using Objects.Players;
using Systems.Templates;
using Tools.Agent;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Objects.Agents
{
    [Flags]
    public enum RtsAgentCommandType
    {
        Move = 1,
        Attack = 2,
        Reclaim = 4,
        Assist = 8,
        BuildTankUnit = 16,
        BuildEngineerUnit = 32,
        BuildFactoryUnit = 64,
        SelectClosestUnit = 128,
        SelectUnitsOfClosestUnitTypeInArea = 256,
        SelectUnitsInArea = 512
    }
    
    public class RtsAgent : DebuggableAgent
    {
        [SerializeField] private BufferSensorComponent ownUnitSensorComponent;
        [SerializeField] private BufferSensorComponent objectSensorComponent;
        [SerializeField] private AIPlayer aiPlayer;
        [SerializeField] private float selectionAreaSize;

        public override void CollectObservations(VectorSensor sensor)
        {
            RtsAgentObservation rtsAgentObservation = aiPlayer.currentObservation;
            
            observationsDebugSet.Add("Time", $"{rtsAgentObservation.vectorObservations[0]:.0}");
            
            foreach (float observation in rtsAgentObservation.vectorObservations)
            {
                sensor.AddObservation(observation);
            }

            foreach (float[] objectObservation in rtsAgentObservation.observationBuffers[0])     
            {
                objectSensorComponent.AppendObservation(objectObservation);
            }
        
            foreach (float[] objectObservation in rtsAgentObservation.observationBuffers[1])     
            {
                ownUnitSensorComponent.AppendObservation(objectObservation);
            }

            BroadcastObservationsCollected();
        }
    
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ActionBuffers actionBuffers = actionsOut;
            
            ActionSegment<int> actionBuffersDiscreteActions = actionBuffers.DiscreteActions;
            ActionSegment<float> actionBuffersContinuousActions = actionBuffers.ContinuousActions;
            
            actionBuffersDiscreteActions[0] = 9;
            actionBuffersDiscreteActions[1] = 1;
            actionBuffersContinuousActions[0] = 100;
            actionBuffersContinuousActions[1] = 100;

            bool orderActionRecorded = false;
            
            if (Input.GetKey(KeyCode.Keypad0))
            {
                actionBuffersDiscreteActions[0] = 0;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad1))
            {
                actionBuffersDiscreteActions[0] = 1;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad2))
            {
                actionBuffersDiscreteActions[0] = 2;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad3))
            {
                actionBuffersDiscreteActions[0] = 3;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad4))
            {
                actionBuffersDiscreteActions[0] = 4;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad5))
            {
                actionBuffersDiscreteActions[0] = 5;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad6))
            {
                actionBuffersDiscreteActions[0] = 6;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad7))
            {
                actionBuffersDiscreteActions[0] = 7;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad8))
            {
                actionBuffersDiscreteActions[0] = 8;
                orderActionRecorded = true;
            }
            else if (Input.GetKey(KeyCode.Keypad9))
            {
                actionBuffersDiscreteActions[0] = 9;
                orderActionRecorded = true;
            }

            if (orderActionRecorded)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    actionBuffersDiscreteActions[1] = 0;
                }
                
                Vector3 mouseAction = aiPlayer.environment.transform.InverseTransformPoint(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition)) / aiPlayer.environment.halfGroundSize;
                
                actionBuffersContinuousActions[0] = mouseAction.x;
                actionBuffersContinuousActions[1] = mouseAction.z;
            }
        } 
    
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            // RtsAgentCommandType rtsAgentCommandType;
            //
            // foreach (Unit aiPlayerSelectedUnit in aiPlayer.selectedUnits)
            // {
            //     foreach (OrderType orderType in aiPlayerSelectedUnit.orderTypeExecutionModulesTable.Keys)
            //     {
            //         if (orderType)
            //         {
            //             
            //         }
            //     }
            // }
            //
            // aiPlayer.selectedUnits.ForEach(x => x);
            //
            // foreach (Unit selectedUnit in rtsAgentCommandType.g)
            // {
            //     actionMask.SetActionEnabled(0, 1, i < aiPlayer.ownedUnits.Count);
            // }
        }
    
        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
        
            ActionSegment<float> continuousActions = actions.ContinuousActions;
            ActionSegment<int> discreteActions = actions.DiscreteActions;
            
            Vector2 cursorAction = new Vector2(
                Mathf.Clamp(continuousActions[0], -1f, 1f),
                Mathf.Clamp(continuousActions[1], -1f, 1f));
            
            aiPlayer.cursorTransform.localPosition = new Vector3(
                aiPlayer.environment.halfGroundSize * cursorAction.x,
                0,
                aiPlayer.environment.halfGroundSize * cursorAction.y);
            
            
            RtsAgentCommandType rtsAgentCommandType = (RtsAgentCommandType) Mathf.Pow(2, discreteActions[0]);
            bool shiftCommand = discreteActions[1] == 1;
            
            if (rtsAgentCommandType > RtsAgentCommandType.BuildFactoryUnit)
            {
                PerformUnitSelectionCommand(rtsAgentCommandType, shiftCommand);
            }
            else
            {
                PerformNewOrderCommand(rtsAgentCommandType, shiftCommand);
            }
        }

        private void PerformUnitSelectionCommand(RtsAgentCommandType rtsAgentCommandType, bool shiftCommand)
        {
            if (!shiftCommand)
            {
                aiPlayer.selectedUnits.Clear();
            }

            if (rtsAgentCommandType == RtsAgentCommandType.SelectClosestUnit)
            {
                Unit targetUnit = null;
                float minFoundDistance = float.MaxValue;
                float currentDistance;

                foreach (Unit aiPlayerOwnedUnit in aiPlayer.ownedUnits)
                {
                    currentDistance = Vector3.Distance(aiPlayerOwnedUnit.transform.position, aiPlayer.cursorTransform.position);

                    if (currentDistance < minFoundDistance && !aiPlayer.selectedUnits.Contains(aiPlayerOwnedUnit))
                    {
                        minFoundDistance = currentDistance;
                        targetUnit = aiPlayerOwnedUnit;
                    }
                }
                
                aiPlayer.selectedUnits.Add(targetUnit);
            }
            else
            {
                List<Unit> ownedUnitsInArea = new List<Unit>();
                
                foreach (Unit aiPlayerOwnedUnit in aiPlayer.ownedUnits)
                {
                    if (aiPlayerOwnedUnit.transform.position.x <= aiPlayer.cursorTransform.position.x + selectionAreaSize &&
                        aiPlayerOwnedUnit.transform.position.x >= aiPlayer.cursorTransform.position.x - selectionAreaSize &&
                        aiPlayerOwnedUnit.transform.position.z <= aiPlayer.cursorTransform.position.z + selectionAreaSize &&
                        aiPlayerOwnedUnit.transform.position.z >= aiPlayer.cursorTransform.position.z - selectionAreaSize)
                    {
                        ownedUnitsInArea.Add(aiPlayerOwnedUnit);
                    }
                }

                if (rtsAgentCommandType == RtsAgentCommandType.SelectUnitsInArea)
                {
                    aiPlayer.selectedUnits.AddRange(ownedUnitsInArea.Where(x => !aiPlayer.selectedUnits.Contains(x)));
                }
                else if (rtsAgentCommandType == RtsAgentCommandType.SelectUnitsOfClosestUnitTypeInArea)
                {
                    Unit targetUnit = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;

                    foreach (Unit ownedUnitInArea in ownedUnitsInArea)
                    {
                        currentDistance = Vector3.Distance(ownedUnitInArea.transform.position, aiPlayer.cursorTransform.position);

                        if (currentDistance < minFoundDistance)
                        {
                            minFoundDistance = currentDistance;
                            targetUnit = ownedUnitInArea;
                        }
                    }

                    if (targetUnit != null)
                    {
                        aiPlayer.selectedUnits.AddRange(ownedUnitsInArea.Where(x => 
                            x.unitTemplate == targetUnit.unitTemplate && 
                            !aiPlayer.selectedUnits.Contains(x)));
                    }
                }
            }
        }
        
        private void PerformNewOrderCommand(RtsAgentCommandType rtsAgentCommandType, bool shiftCommand)
        {
            if (rtsAgentCommandType == RtsAgentCommandType.Move)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Move)).ToList();
                
                if (capableUnits.Count > 0)
                {
                    aiPlayer.CreateOrder(
                        OrderType.Move, 
                        aiPlayer.environment.ground, 
                        capableUnits, 
                        shiftCommand,
                        aiPlayer.cursorTransform.localPosition);
                }

            }
            else if (rtsAgentCommandType == RtsAgentCommandType.Attack)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Attack)).ToList();
                
                if (capableUnits.Count > 0)
                {
                    Unit targetUnit = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;
    
                    foreach (Player playerInEnvironment in aiPlayer.environment.players)
                    {
                        if (playerInEnvironment.teamId != aiPlayer.teamId)
                        {
                            foreach (Unit enemyUnit in playerInEnvironment.ownedUnits)
                            {
                                currentDistance = Vector3.Distance(enemyUnit.transform.position, aiPlayer.cursorTransform.position);

                                if (currentDistance < minFoundDistance)
                                {
                                    minFoundDistance = currentDistance;
                                    targetUnit = enemyUnit;
                                }
                            }
                        }
                    }
                
                    if (targetUnit != null)
                    {
                        aiPlayer.CreateOrder(
                            OrderType.Attack, 
                            targetUnit.transform, 
                            capableUnits, 
                            shiftCommand);
                    }
                }
            }
            else if (rtsAgentCommandType == RtsAgentCommandType.Reclaim)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Reclaim)).ToList();
                
                if (capableUnits.Count > 0)
                {
                    Reclaim targetReclaim = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;

                    foreach (Reclaim reclaim in aiPlayer.environment.reclaims)
                    {
                        currentDistance = Vector3.Distance(reclaim.transform.position, aiPlayer.cursorTransform.position);

                        if (currentDistance < minFoundDistance)
                        {
                            minFoundDistance = currentDistance;
                            targetReclaim = reclaim;
                        }
                    }

                    if (targetReclaim != null)
                    {
                        aiPlayer.CreateOrder(
                            OrderType.Reclaim, 
                            targetReclaim.transform, 
                            capableUnits, 
                            shiftCommand);
                    }
                }
            }
            else if (rtsAgentCommandType == RtsAgentCommandType.Assist)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Assist)).ToList();

                if (capableUnits.Count > 0)
                {
                    Unit targetUnit = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;
    
                    foreach (Player playerInEnvironment in aiPlayer.environment.players)
                    {
                        if (playerInEnvironment.teamId == aiPlayer.teamId)
                        {
                            foreach (Unit alliedUnit in playerInEnvironment.ownedUnits)
                            {
                                currentDistance = Vector3.Distance(alliedUnit.transform.position, aiPlayer.cursorTransform.position);

                                if (currentDistance < minFoundDistance)
                                {
                                    minFoundDistance = currentDistance;
                                    targetUnit = alliedUnit;
                                }
                            }
                        }
                    }
                
                    if (targetUnit != null)
                    {
                        aiPlayer.CreateOrder(
                            OrderType.Assist, 
                            targetUnit.transform, 
                            capableUnits, 
                            shiftCommand);
                    }
                }
            }
            else if (rtsAgentCommandType == RtsAgentCommandType.BuildFactoryUnit)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.BuildFactoryUnit)).ToList();

                if (capableUnits.Count > 0 && 
                    Physics.OverlapBox(aiPlayer.cursorTransform.position, Vector3.one * aiPlayer.factoryTemplate.size, Quaternion.identity, aiPlayer.interactableLayerMask).Length < 1)
                {
                    aiPlayer.CreateOrder(
                        OrderType.BuildFactoryUnit, 
                        aiPlayer.environment.ground, 
                        capableUnits, 
                        shiftCommand,
                        aiPlayer.cursorTransform.localPosition);
                }
            }
            else if (rtsAgentCommandType == RtsAgentCommandType.BuildTankUnit)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.BuildTankUnit)).ToList();

                if (capableUnits.Count > 0)
                {
                    foreach (Unit capableUnit in capableUnits)
                    {
                        aiPlayer.CreateOrder(
                            OrderType.BuildTankUnit, 
                            capableUnit.transform, 
                            new List<Unit>{capableUnit}, 
                            shiftCommand);
                    }
                }
            }
            else if (rtsAgentCommandType == RtsAgentCommandType.BuildEngineerUnit)
            {
                List<Unit> capableUnits = aiPlayer.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.BuildEngineerUnit)).ToList();

                if (capableUnits.Count > 0)
                {
                    foreach (Unit capableUnit in capableUnits)
                    {
                        aiPlayer.CreateOrder(
                            OrderType.BuildEngineerUnit, 
                            capableUnit.transform, 
                            new List<Unit>{capableUnit}, 
                            shiftCommand);
                    }
                }
            }
        }
    }
}