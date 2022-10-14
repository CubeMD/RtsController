﻿using System;
using System.Collections.Generic;
using Templates;
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
    [SerializeField] private float timeToReset;

    private float timeSinceReset;
    
    private void Awake()
    {
        halfGroundSize = ground.localScale.x / 2;

        foreach (RtsAgent rtsAgent in agents)
        {
            OnEnvironmentReset += rtsAgent.EndEpisode;
        }
        
        SpawnStartingReclaim();
    }

    public void Update()
    {
        timeSinceReset += Time.deltaTime;
        
        if (timeSinceReset >= timeToReset)
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
            reclaim.SetEnvironmentOnDestroy(this);
            reclaim.SetRandomGaussianAmount(reclaimMinMax);
        }
    }
}