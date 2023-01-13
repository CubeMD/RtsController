using System;
using System.Collections.Generic;
using Objects.Players;
using Systems.Interfaces;
using Systems.Templates;
using Tools.Utilities;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Objects
{
    public class Environment : MonoBehaviour
    {
        public event Action OnEnvironmentReset; 
    
        [Header("Ground")]
        [HideInInspector] public float halfGroundSize;
    
        public Transform ground;

        [Header("Reclaim")]
        public List<Reclaim> reclaims;
    
        [SerializeField] private Vector2 startingReclaimMinMax;
        [SerializeField] private Reclaim reclaimPrefab;
        [SerializeField] private int numStartingReclaim;
    
        [Header("Agents")]
        public List<Player> players;
    
        [Header("Unit")]
        [SerializeField] public Unit unitPrefab;
        [SerializeField] private UnitTemplate startingUnitTemplate;
        [SerializeField] private int numStartingUnits = 1;
        [SerializeField] private float startingUnitSpread = 20;

        [Header("Time")]
        public float timeWhenReset;
        [HideInInspector] public float timeSinceReset;


        private void Awake()
        {
            Academy.Instance.AutomaticSteppingEnabled = false;
        
            halfGroundSize = ground.localScale.x / 2;

            foreach (Player player in players)
            {
                OnEnvironmentReset += player.HandleEnvironmentReset;
            }
        
            SpawnStartingReclaim();
            SpawnStartingUnits();
        }
    
        private void OnDestroy()
        {
            foreach (Player player in players)
            {
                OnEnvironmentReset -= player.HandleEnvironmentReset;
            }
        }

        public void FixedUpdate()
        {
            timeSinceReset += Time.fixedDeltaTime;
        
            if (timeSinceReset >= timeWhenReset)
            {
                ResetEnvironment();
            }
        }
    
        public void ResetEnvironment()
        {
            foreach (Reclaim reclaim in reclaims)
            {
                players[0].AddReward(-reclaim.Amount / 10);
            }
        
            OnEnvironmentReset?.Invoke();
            timeSinceReset = 0;
            reclaims.Clear();
            Resources.UnloadUnusedAssets();

            SpawnStartingReclaim();
            SpawnStartingUnits();
        }
    
        public void SpawnStartingReclaim()
        {
            for (int i = 0; i < numStartingReclaim; i++)
            {
                Vector3 localPosition = new Vector3(
                    Random.Range(-halfGroundSize, halfGroundSize),
                    0,
                    Random.Range(-halfGroundSize, halfGroundSize));
            
                Reclaim reclaim = ObjectPooler.InstantiateGameObject(reclaimPrefab, transform.localPosition + localPosition, Quaternion.identity, transform);
                reclaim.OnDestroyableDestroy += HandleReclaimDestroyed;
                reclaims.Add(reclaim);
                reclaim.SetEnvironment(this);
                reclaim.SetRandomGaussianAmount(startingReclaimMinMax);
            }
        }

        public void HandleReclaimDestroyed(IDestroyable destroyable)
        {
            destroyable.OnDestroyableDestroy -= HandleReclaimDestroyed;
            reclaims.Remove(destroyable.GetGameObject().GetComponent<Reclaim>());
        
            if (reclaims.Count < 1 && timeSinceReset < timeWhenReset)
            {
                players[0].AddReward((1f - timeSinceReset / timeWhenReset) * 1000f);
                ResetEnvironment();
            }
        }
    
        private void SpawnStartingUnits()
        {
            foreach (Player player in players)
            {
                for (int i = 0; i < numStartingUnits; i++)
                {
                    Vector3 localPosition = new Vector3(
                        Random.Range(-startingUnitSpread, startingUnitSpread), 
                        0, 
                        Random.Range(-startingUnitSpread, startingUnitSpread));

                    player.SpawnUnit(startingUnitTemplate, localPosition);
                }
            }
        }
    }
}