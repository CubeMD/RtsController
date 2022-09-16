using System.Collections.Generic;
using Targets;
using Units;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float halfGroundSize;
    
    [SerializeField] private Transform ground;

    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numBaseReclaim;

    [SerializeField] private Commander commanderPrefab;


    private readonly List<Reclaim> reclaim = new List<Reclaim>();
    public IEnumerable<Reclaim> Reclaim => reclaim;

    [SerializeField] private List<RtsAgent> agents;
    
    private readonly Dictionary<RtsAgent, List<Unit>> agentUnits = new Dictionary<RtsAgent, List<Unit>>();
    
    public void Initialize()
    {
        halfGroundSize = ground.localScale.x / 2;
        
        for (int i = 0; i < numBaseReclaim; i++)
        {
            SpawnReclaim(reclaimPrefab);
        }

        foreach (RtsAgent rtsAgent in agents)
        {
            agentUnits.Add(rtsAgent, new List<Unit>());
            SpawnUnit(commanderPrefab, rtsAgent);
            SpawnUnit(commanderPrefab, rtsAgent);
            SpawnUnit(commanderPrefab, rtsAgent);
            SpawnUnit(commanderPrefab, rtsAgent);
            SpawnUnit(commanderPrefab, rtsAgent);
        }
    }
    
    public bool UnitBelongsToAgent(Unit unit, RtsAgent agent)
    {
        if (agentUnits.ContainsKey(agent))
        {
            return agentUnits[agent].Contains(unit);
        }

        return false;
    }
    private void SpawnReclaim(Reclaim r)
    {
        reclaim.Add(Instantiate(r, transform));
    }
    
    private void SpawnUnit(Unit unit, RtsAgent owner)
    {
        agentUnits[owner].Add(Instantiate(unit, transform));
    }
    
    
    public void Reset()
    {
        ResetReclaim();
        ResetUnits();
    }

    private void ResetUnits()
    {
        foreach (KeyValuePair<RtsAgent,List<Unit>> agentUnit in agentUnits)
        {
            foreach (Unit unit in agentUnit.Value)
            {
                unit.Reset();
            }
        }
    }
    private void ResetReclaim()
    {
        foreach (Reclaim r in reclaim)
        {
            r.Reset();
        }
    }
}