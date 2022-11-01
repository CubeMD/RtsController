using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public event Action OnEnvironmentReset; 

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
            OnEnvironmentReset += rtsAgent.EndEpisode;
        }
    }

    private void OnDestroy()
    {
        foreach (RtsAgent rtsAgent in agents)
        {
            OnEnvironmentReset -= rtsAgent.EndEpisode;
        }
    }

    public void Update()
    {
        timeSinceReset += Time.deltaTime;
        
        if (timeSinceReset >= timeWhenReset)
        {
            ResetEnvironment();
        }
    }

    public void ResetEnvironment()
    {
        OnEnvironmentReset?.Invoke();
        timeSinceReset = 0;
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

            Reclaim reclaim = Instantiate(reclaimPrefab, transform.localPosition + localPosition, Quaternion.identity, transform);
            reclaim.SetEnvironment(this);
            reclaim.SetRandomGaussianAmount(reclaimMinMax);
        }
    }
}