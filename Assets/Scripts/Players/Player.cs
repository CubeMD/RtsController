using System;
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
        [HideInInspector] public Vector3 startingPosition;
        
        public int teamId;
        public MatchManager matchManager;
        public EconomyManager economyManager;
        public UnitManager unitManager;

        private readonly List<AgentStep> episodeTrajectory = new List<AgentStep>();

        public virtual void ResetPlayer()
        {
            economyManager.ResetEconomyManager();
            unitManager.ResetUnitManager();
            episodeTrajectory.Clear();
        }
    }
}
