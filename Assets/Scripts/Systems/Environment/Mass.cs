using System;
using Systems.ObjectPooling;
using UnityEngine;
using Utilities.Attributes;
using Random = UnityEngine.Random;

namespace Systems.Environment
{
    public class Mass : MonoBehaviour
    {
        [StaticDomainReloadField]
        public static event Action<Mass> OnAnyMassDestroyed; 
        
        public event Action<Mass> OnMassDestroyed;
        
        [SerializeField] 
        private MeshRenderer meshRenderer;
        
        [SerializeField] [ReadOnly]
        private float massAmount = 1;
        public float MassAmount => massAmount;
        
        public void GenerateRandomMassAmount()
        {
            massAmount = Mathf.Lerp(EnvironmentGlobalSettings.MassAmountMinMax.x,
                EnvironmentGlobalSettings.MassAmountMinMax.y, Random.value);
            UpdateMassMaterialColor();
        }

        public void SetMassAmount(float amount)
        {
            massAmount = amount;
            UpdateMassMaterialColor();
        }

        private void UpdateMassMaterialColor()
        {
            meshRenderer.material.color = EnvironmentGlobalSettings.GetMassGradientColor(massAmount);
        }

        public void DeductMassAmount(float amountToDeduct)
        {
            massAmount -= amountToDeduct;
            
            if (massAmount <= 0)
            {
                DestroyMass();
                return;
            }

            UpdateMassMaterialColor();
        }

        public void DestroyMass()
        {
            ObjectPooler.PoolGameObject(this);
            OnMassDestroyed?.Invoke(this);
            OnAnyMassDestroyed?.Invoke(this);
        }
    }
}