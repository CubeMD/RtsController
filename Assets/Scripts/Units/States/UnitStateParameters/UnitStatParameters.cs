using System;
using UnityEngine;

namespace Units.States.UnitStateParameters
{
    [Serializable]
    public class UnitStatParameters
    {
        [SerializeField] private float energyCost;
        [SerializeField] private float massCost;
        [SerializeField] private float maxHp;
        
        public float CurrentHp { get; private set; }
        public float ConstructionPercentage => currentMass / massCost;
        public float MassDumped { get; private set; }
        public bool IsConstructed { get; private set; } = false;

        private float currentMass;
        private float currentEnergy;

        public void DumpMass(float amount)
        {
            MassDumped += amount;
        }

        public void ClearDump()
        {
            MassDumped = 0;
        }

        public void AddCurrentMass(float amount)
        {
            currentMass = Mathf.Min(massCost, currentMass + amount);
        }

        public void AddCurrentEnergy(float amount)
        {
            currentEnergy += amount;
        }

        public void SetCurrentHP(float hp)
        {
            CurrentHp = hp;
        }
        
        public void SetConstructed()
        {
            IsConstructed = true;
            CurrentHp = maxHp;
            currentMass = massCost;
            currentEnergy = energyCost;
        }
    }
}