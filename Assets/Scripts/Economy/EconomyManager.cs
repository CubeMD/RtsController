using UnityEngine;

namespace Economy
{
    public class EconomyManager : MonoBehaviour
    {
        [SerializeField] private float defaultMassAmount;
        [SerializeField] private float defaultEnergyAmount;

        public float MassAmount { get; private set; }
        public float EnergyAmount { get; private set; }

        private void Awake()
        {
            ResetEconomyManager();
        }

        public void ResetEconomyManager()
        {
            MassAmount = defaultMassAmount;
            EnergyAmount = defaultEnergyAmount;
        }

        public void UnitCollectedMass(float amount)
        {
            MassAmount += amount;
        }
    }
}