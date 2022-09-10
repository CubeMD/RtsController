using System;
using System.Collections.Generic;
using Targets;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [HideInInspector] public float halfGroundSize;
        
    [SerializeField] private GameObject ground;

    [SerializeField] private GaussianTarget gaussianTargetPrefab;
    [SerializeField] private int numGaussianTargets;

    [SerializeField] private TerminateTarget terminateTargetPrefab;

    public readonly List<Target> targets = new List<Target>();
        

    public void Initialize()
    {
        halfGroundSize = ground.transform.localScale.x / 2;
            
        for (int i = 0; i < numGaussianTargets; i++)
        {
            GaussianTarget t = Instantiate(gaussianTargetPrefab, transform);
            t.Initialize(halfGroundSize);
            targets.Add(t);
        }

        TerminateTarget tt = Instantiate(terminateTargetPrefab, transform);
        tt.Initialize(halfGroundSize);
        targets.Add(tt);
    }

    private void ResetTargets()
    {
        foreach (Target target in targets)
        {
            target.Reset();
        }
    }

    public void Reset()
    {
        ResetTargets();
    }
}