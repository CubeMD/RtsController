using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Interfaces;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public event Action OnEnvironmentReset; 
    public List<Reclaim> reclaims;
    
    [Header("Ground")]
    [HideInInspector] public float halfGroundSize;
    [SerializeField] private Transform ground;

    [Header("Reclaim")]
    [SerializeField] private Vector2 reclaimMinMax;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numGaussianReclaim;
    
    [Header("Agents")]
    [SerializeField] private List<RtsAgent> agents;
    
    public float timeWhenReset;
    public float timeSinceReset;

    private void Awake()
    {
        halfGroundSize = ground.localScale.x / 2;
        SpawnStartingReclaim();
        
        foreach (RtsAgent rtsAgent in agents)
        {
            OnEnvironmentReset += rtsAgent.HandleEpisodeEnded;
        }
    }

    private void OnDestroy()
    {
        foreach (RtsAgent rtsAgent in agents)
        {
            OnEnvironmentReset -= rtsAgent.HandleEpisodeEnded;
        }
    }

    public void FixedUpdate()
    {
        timeSinceReset += Time.fixedDeltaTime;
        
        if (timeSinceReset >= timeWhenReset)
        {
            foreach (Reclaim reclaim in reclaims)
            {
                agents[0].AddReward(-reclaim.Amount);
            }
            
            ResetEnvironment();
        }
    }

    public void ResetEnvironment()
    {
        OnEnvironmentReset?.Invoke();
        timeSinceReset = 0;
        reclaims.Clear();
        Resources.UnloadUnusedAssets();
        
        SpawnStartingReclaim();
    }

    public void SpawnStartingReclaim()
    {
        for (int i = 0; i < numGaussianReclaim; i++)
        {
            Vector3 localPosition = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize),
                0,
                Random.Range(-halfGroundSize, halfGroundSize));
            
            Reclaim reclaim = ObjectPooler.InstantiateGameObject(reclaimPrefab, transform.localPosition + localPosition, Quaternion.identity, transform);
            reclaim.OnDestroyableDestroy += HandleReclaimDestroyed;
            reclaims.Add(reclaim);
            reclaim.SetEnvironment(this);
            reclaim.SetRandomGaussianAmount(reclaimMinMax);
        }
    }

    public void HandleReclaimDestroyed(IDestroyable destroyable)
    {
        destroyable.OnDestroyableDestroy -= HandleReclaimDestroyed;
        reclaims.Remove(destroyable.GetGameObject().GetComponent<Reclaim>());
        
        if (reclaims.Count < 1 && timeSinceReset < timeWhenReset)
        {
            agents[0].AddReward((1f - timeSinceReset / timeWhenReset) * 1000f);
            ResetEnvironment();
        }
    }
}