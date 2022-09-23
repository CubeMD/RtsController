using System.Collections.Generic;
using Templates;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    private const string COMMANDER_TEMPLATE_ID = "UEFComm";
    
    [Header("Ground")]
    [HideInInspector] public float halfGroundSize;
    [SerializeField] private Transform ground;

    [Header("Reclaim")]
    [SerializeField] private Vector2 reclaimMinMax;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numGaussianReclaim;

    [Header("Unit")]
    [SerializeField] private Unit unitPrefab;
    
    [SerializeField] private List<RtsAgent> agents;
    [SerializeField] private List<UnitTemplate> templates;
    [SerializeField] private int numCommanders = 1;
    
    private readonly List<Reclaim> reclaims = new List<Reclaim>();
    private readonly List<Unit> units = new List<Unit>();
    private readonly Dictionary<RtsAgent, List<Unit>> agentUnits = new Dictionary<RtsAgent, List<Unit>>();
    private readonly Dictionary<string, UnitTemplate> unitIdTable = new Dictionary<string, UnitTemplate>();
    
    private void Awake()
    {
        halfGroundSize = ground.localScale.x / 2;

        foreach (RtsAgent agent in agents)
        {
            agentUnits.Add(agent, new List<Unit>());
        }        

        foreach (UnitTemplate unitTemplate in templates)
        {
            unitIdTable.Add(unitTemplate.id, unitTemplate);
        }
    }

    public bool UnitBelongsToAgent(Unit unit, RtsAgent agent)
    {
        return agentUnits.ContainsKey(agent) && agentUnits[agent].Contains(unit);
    }

    public void Reset()
    {
        ResetReclaim();
        ResetUnits();
    }

    private void ResetUnits()
    {
        foreach (List<Unit> agentUnit in agentUnits.Values)
        {
            foreach (Unit unit in agentUnit)
            {
                Destroy(unit.gameObject);
            }
        }
        
        foreach (RtsAgent rtsAgent in agents)
        {
            for (int i = 0; i < numCommanders; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(-halfGroundSize, halfGroundSize), 
                    0, 
                    Random.Range(-halfGroundSize, halfGroundSize));
            
                units.Add(SpawnUnit(rtsAgent, position, unitIdTable[COMMANDER_TEMPLATE_ID]));
            }
        }
    }

    private Unit SpawnUnit(RtsAgent owner, Vector3 position, UnitTemplate unitTemplate)
    {
        Unit unit = Instantiate(unitPrefab, position, Quaternion.identity, transform);
        unit.SetUnit(unitTemplate, owner);
        unit.OnUnitDestroyed += HandleUnitDestroyed;
        agentUnits[owner].Add(unit);
        return unit;
    }

    private void HandleUnitDestroyed(Unit unit)
    {
        agentUnits[unit.Owner].Remove(unit);
        if (units.Remove(unit))
        {
            unit.OnUnitDestroyed -= HandleUnitDestroyed;
        }
        else
        {
            Debug.LogWarning("Received event for destruction of foreign unit", unit);
        }
    }
    
    private void ResetReclaim()
    {
        foreach (Reclaim r in reclaims)
        {
            Destroy(r.gameObject);
        }

        for (int i = 0; i < numGaussianReclaim; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize), 
                0, 
                Random.Range(-halfGroundSize, halfGroundSize));
            
            reclaims.Add(SpawnGaussianReclaim(position));
        }
    }
    
    private Reclaim SpawnGaussianReclaim(Vector3 position)  
    {
        Reclaim naturalReclaim = Instantiate(reclaimPrefab, position, Quaternion.identity, transform);
        naturalReclaim.SetRandomGaussianAmount(reclaimMinMax);
        naturalReclaim.OnReclaimDestroyed += HandleReclaimDestroyed;
        return naturalReclaim;
    }
    
    private void HandleReclaimDestroyed(Reclaim reclaim)
    {
        if (reclaims.Remove(reclaim))
        {
            reclaim.OnReclaimDestroyed -= HandleReclaimDestroyed;
        }
        else
        {
            Debug.LogWarning("Received event for destruction of foreign reclaim", reclaim);
        }
    }
}