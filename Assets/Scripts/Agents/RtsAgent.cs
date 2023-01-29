using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Players;
using Units;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utilities.Agent;

namespace Agents
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

            const int keypadZeroIndex = (int)KeyCode.Keypad0;
            
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKey((KeyCode) (keypadZeroIndex + i)))
                {
                    actionBuffersDiscreteActions[0] = i;
                    Debug.Log($"Key {i}");
                    
                    if (!Input.GetKey(KeyCode.RightShift))
                    {
                        actionBuffersDiscreteActions[1] = 0;
                        Debug.Log("Shift not pressed");
                    }
                    else
                    {
                        Debug.Log("Shift is pressed");
                    }
                    
                    Vector3 mouseAction = aiPlayer.matchManager.transform.InverseTransformPoint(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition)) / aiPlayer.matchManager.HalfGroundSize;
                
                    actionBuffersContinuousActions[0] = mouseAction.x;
                    actionBuffersContinuousActions[1] = mouseAction.z;
                    break;
                }
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
                aiPlayer.matchManager.HalfGroundSize * cursorAction.x,
                0,
                aiPlayer.matchManager.HalfGroundSize * cursorAction.y);
            
            
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
            UnitManager unitManager = aiPlayer.unitManager;
            
            if (!shiftCommand)
            {
                unitManager.SelectedUnits.Clear();
            }
            
            if (rtsAgentCommandType == RtsAgentCommandType.SelectClosestUnit)
            {
                Unit targetUnit = null;
                float minFoundDistance = float.MaxValue;
                float currentDistance;

                foreach (Unit aiPlayerOwnedUnit in unitManager.OwnedUnits)
                {
                    currentDistance = Vector3.Distance(aiPlayerOwnedUnit.transform.position, aiPlayer.cursorTransform.position);

                    if (currentDistance < minFoundDistance && !unitManager.SelectedUnits.Contains(aiPlayerOwnedUnit))
                    {
                        minFoundDistance = currentDistance;
                        targetUnit = aiPlayerOwnedUnit;
                    }
                }
                
                unitManager.AddSelectedUnit(targetUnit);
            }
            else
            {
                List<Unit> ownedUnitsInArea = new List<Unit>();
                
                foreach (Unit aiPlayerOwnedUnit in unitManager.OwnedUnits)
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
                    unitManager.AddSelectedUnits(ownedUnitsInArea);
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
                        unitManager.AddSelectedUnits(ownedUnitsInArea.Where(x => x.UnitType == targetUnit.UnitType).ToList());
                    }
                }
            }
        }
        
        private void PerformNewOrderCommand(RtsAgentCommandType rtsAgentCommandType, bool shiftCommand)
        {
            if (rtsAgentCommandType == RtsAgentCommandType.Move)
            {
                List<Unit> capableUnits = aiPlayer.unitManager.GetSelectedUnitsByOrder(OrderType.Move);

                foreach (Unit unit in capableUnits)
                {
                    if (!(unit is IMove movableUnit))
                    {
                        Debug.LogError("Something is off. Non-movable unit can execute move order type");
                        continue;
                    }
                    
                    movableUnit.Move(aiPlayer.cursorTransform.position, shiftCommand);
                }
            }
            /*else if (rtsAgentCommandType == RtsAgentCommandType.Attack)
            {
                List<Unit> capableUnits = aiPlayer.unitManager.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Attack)).ToList();
                
                if (capableUnits.Count > 0)
                {
                    Unit targetUnit = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;
    
                    foreach (Player playerInEnvironment in aiPlayer.matchManager.players)
                    {
                        if (playerInEnvironment.teamId != aiPlayer.teamId)
                        {
                            foreach (Unit enemyUnit in playerInEnvironment.unitManager.ownedUnits)
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
                        Order order = new TargetedOrder(OrderType.Attack, capableUnits, targetUnit.transform);
                    
                        foreach (Unit capableUnit in capableUnits)
                        {
                            capableUnit.AssignOrder(order, shiftCommand);
                        }
                    }
                }
            }*/
            else if (rtsAgentCommandType == RtsAgentCommandType.Reclaim)
            {
                List<Unit> capableUnits = aiPlayer.unitManager.GetSelectedUnitsByOrder(OrderType.Reclaim);
                
                if (capableUnits.Count > 0)
                {
                    if (aiPlayer.matchManager.TryGetClosesReclaimToPosition(aiPlayer.cursorTransform.position,
                            out Reclaim closestReclaim))
                    {
                        foreach (Unit capableUnit in capableUnits)
                        {
                            if (!(capableUnit is IReclaim engineer))
                            {
                                Debug.LogError("Something is off. Non-engineering unit can execute reclaim order type");
                                continue;
                            }
                            
                            engineer.Reclaim(closestReclaim, shiftCommand);
                        }
                    }
                }
            }
            /*else if (rtsAgentCommandType == RtsAgentCommandType.Assist)
            {
                List<Unit> capableUnits = aiPlayer.unitManager.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.Assist)).ToList();

                if (capableUnits.Count > 0)
                {
                    Unit targetUnit = null;
                    float minFoundDistance = float.MaxValue;
                    float currentDistance;
    
                    foreach (Player playerInEnvironment in aiPlayer.matchManager.players)
                    {
                        if (playerInEnvironment.teamId == aiPlayer.teamId)
                        {
                            foreach (Unit alliedUnit in playerInEnvironment.unitManager.ownedUnits)
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
                        Order order = new TargetedOrder(OrderType.Assist, capableUnits, targetUnit.transform);
                    
                        foreach (Unit capableUnit in capableUnits)
                        {
                            capableUnit.AssignOrder(order, shiftCommand);
                        }
                    }
                }
            }*/
             else if (rtsAgentCommandType == RtsAgentCommandType.BuildFactoryUnit)
             {
                 List<Unit> capableUnits = aiPlayer.unitManager.GetSelectedUnitsByOrder(OrderType.BuildFactory);

                 if (capableUnits.Count > 0 && aiPlayer.unitManager.CanBuildUnitTypeAtPosition(UnitType.Factory, aiPlayer.cursorTransform.position))
                 {
                     foreach (Unit capableUnit in capableUnits)
                     {
                         if (!(capableUnit is IBuild engineer))
                         {
                             Debug.LogError("Something is off. Non-engineering unit can execute build factory order type");
                             continue;
                         }
                            
                         engineer.Build(UnitType.Factory, aiPlayer.cursorTransform.position, shiftCommand);
                     }
                 }
             }
             // else if (rtsAgentCommandType == RtsAgentCommandType.BuildTankUnit)
             // {
             //     foreach (Unit capableUnit in aiPlayer.unitManager.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.BuildTank)))
             //     {
             //         Order order = new TargetedOrder(OrderType.BuildTank, new List<Unit>{capableUnit}, capableUnit.transform);
             //             
             //         capableUnit.AssignOrder(order, shiftCommand);
             //     }
             // }
             // else if (rtsAgentCommandType == RtsAgentCommandType.BuildEngineerUnit)
             // {
             //     foreach (Unit capableUnit in aiPlayer.unitManager.selectedUnits.Where(x => x.CanExecuteOrderType(OrderType.BuildEngineer)))
             //     {
             //         Order order = new TargetedOrder(OrderType.BuildEngineer, new List<Unit>{capableUnit}, capableUnit.transform);
             //             
             //         capableUnit.AssignOrder(order, shiftCommand);
             //     }
             // }
        }
    }
}