using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgentsDebugTool.Duplicator
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private GameObject ground;
        [HideInInspector] public float halfGroundSize;
        
        [SerializeField] private Target targetPrefab;
        [SerializeField] private int numTargets;
    
        public readonly List<Target> targets = new List<Target>();
        

        public void Initialize()
        {
            halfGroundSize = ground.transform.localScale.x / 2;
            
            for (int i = 0; i < numTargets; i++)
            {
                Target t = Instantiate(targetPrefab, transform);
                t.Initialize(halfGroundSize);
                targets.Add(t);
            }
        }

        public void ResetTargets()
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
}