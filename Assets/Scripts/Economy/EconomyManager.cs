using UnityEngine;

namespace Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public float massAmount;
        public float energyAmount;

        [SerializeField] private float defaultMassAmount;
        [SerializeField] private float defaultEnergyAmount;
    
        public void ResetEconomyManager()
        {
            massAmount = defaultMassAmount;
            energyAmount = defaultEnergyAmount;
        }

        public void UnitCollectedMass(float amount)
        {
            massAmount += amount;
        }
    }
}