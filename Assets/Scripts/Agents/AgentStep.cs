using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    public class AIPlayerAction
    {
        public UnitSelectorAgentAction unitSelectorAgentAction;
        public PositionSelectorAgentAction positionSelectorAgentAction;

        public AIPlayerAction(UnitSelectorAgentAction unitSelectorAgentAction, PositionSelectorAgentAction positionSelectorAgentAction)
        {
            this.unitSelectorAgentAction = unitSelectorAgentAction;
            this.positionSelectorAgentAction = positionSelectorAgentAction;
        }

        public AIPlayerAction()
        {
            
        }
    }
    
    public class UnitSelectorAgentAction
    {
        public List<Unit> selectedUnits;
            
        public UnitSelectorAgentAction(List<Unit> selectedUnits)
        {
            this.selectedUnits = selectedUnits;
        }
    }
    
    public class PositionSelectorAgentAction
    {
        public Vector2 cursorAction;
        public bool shiftAction;
            
        public PositionSelectorAgentAction(Vector2 cursorAction, bool shiftAction)
        {
            this.cursorAction = cursorAction;
            this.shiftAction = shiftAction;
        }
    }
    
    public class AIPlayerObservation
    {
        public UnitSelectorAgentObservation unitSelectorAgentObservation;
        public PositionSelectorAgentObservation positionSelectorAgentObservation;
            
        public AIPlayerObservation(UnitSelectorAgentObservation unitSelectorAgentObservation, PositionSelectorAgentObservation positionSelectorAgentObservation)
        {
            this.unitSelectorAgentObservation = unitSelectorAgentObservation;
            this.positionSelectorAgentObservation = positionSelectorAgentObservation;
        }
    }
    
    public class UnitSelectorAgentObservation
    {
        public float[] vectorObservations;
        public List<List<float[]>> observations;
            
        public UnitSelectorAgentObservation(float[] vectorObservations, List<List<float[]>> observations)
        {
            this.vectorObservations = vectorObservations;
            this.observations = observations;
        }
    }
    
    public class PositionSelectorAgentObservation
    {
        public float[] vectorObservations;
        public List<List<float[]>> observations;
            
        public PositionSelectorAgentObservation(float[] vectorObservations, List<List<float[]>> observations)
        {
            this.vectorObservations = vectorObservations;
            this.observations = observations;
        }
    }

    public class AgentStep
    {
        public AIPlayerAction aiPlayerAction;
        public AIPlayerObservation aiPlayerObservation;
    }
}