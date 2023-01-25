using System.Collections.Generic;
using Agents;
using Economy;
using Orders;
using Units;
using UnityEngine;

namespace Players
{
    public abstract class Player : MonoBehaviour
    {
        public Environment environment;
        public int teamId;

        public MapManager mapManager;
        public EconomyManager economyManager;
        public UnitManager unitManager;

        private readonly List<AgentStep> episodeTrajectory = new List<AgentStep>();
        
        public virtual void HandleEnvironmentReset()
        {
            ResetPlayer();
        }
        
        public virtual void ResetPlayer()
        {
            economyManager.ResetEconomyManager();
            unitManager.ResetUnitManager();
            episodeTrajectory.Clear();
        }
    }
}
