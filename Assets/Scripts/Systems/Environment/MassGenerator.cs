using System;
using System.Collections.Generic;
using Systems.ObjectPooling;
using UnityEngine;
using Utilities.Attributes;
using Random = UnityEngine.Random;

namespace Systems.Environment
{
    public class MassGenerator : MonoBehaviour
    {
        [StaticDomainReloadField]
        public static event Action<MassGenerator> OnAllMassCollected;

        private readonly List<Mass> spawnedMass = new List<Mass>();
        
        private void Awake()
        {
            Mass.OnAnyMassDestroyed += HandleAnyMassDestroyed;
            SpawnMass();
        }

        private void OnDestroy()
        {
            Mass.OnAnyMassDestroyed -= HandleAnyMassDestroyed;
        }

        private void SpawnMass()
        {
            // Keep it here instead of awake for the future training randomization
            Vector2 halfGroundSize = new Vector2(
                EnvironmentGlobalSettings.GroundSize.x / 2,
                EnvironmentGlobalSettings.GroundSize.y / 2);

            for (int i = 0; i < EnvironmentGlobalSettings.InitialMassAmount; i++)
            {
                Vector3 generatedLocalPosition = new Vector3(
                    Random.Range(-halfGroundSize.x, halfGroundSize.x),
                    0,
                    Random.Range(-halfGroundSize.y, halfGroundSize.y));
                
                Mass mass = ObjectPooler.Instantiate(EnvironmentGlobalSettings.Mass, transform.localPosition + generatedLocalPosition, Quaternion.identity);
                spawnedMass.Add(mass);
                mass.GenerateRandomMassAmount();
            }
        }

        private void HandleAnyMassDestroyed(Mass mass)
        {
            spawnedMass.Remove(mass);
            
            if (spawnedMass.Count < 1)
            {
                OnAllMassCollected?.Invoke(this);
            }
        }

        private void HandleEnvironmentReset()
        {
            foreach (Mass mass in spawnedMass)
            {
                ObjectPooler.PoolGameObject(mass);
            }
            
            spawnedMass.Clear();
            
            Resources.UnloadUnusedAssets();

            SpawnMass();
        }
    }
}