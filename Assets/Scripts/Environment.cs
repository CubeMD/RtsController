using System;
using System.Collections.Generic;
using Templates;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public Action<Reclaim> onReclaimDestroyed;

    private const string commanderTemplateId = "UEFComm";
    
    [Header("Ground")]
    [HideInInspector] public float halfGroundSize;
    [SerializeField] private Transform ground;

    [Header("Reclaim")]
    [SerializeField] private Vector2 reclaimMinMax;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numGaussianReclaim;

    [SerializeField] private Unit unitPrefab;
    
    [SerializeField] private List<RtsAgent> agents;
    [SerializeField] private List<UnitTemplate> templates;
    [SerializeField] private int numCommanders = 1;
    
    private List<Reclaim> reclaims = new List<Reclaim>();
    public List<Unit> units = new List<Unit>();

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

        onReclaimDestroyed += OnReclaimDestroyed;
    }

    public bool UnitBelongsToAgent(Unit unit, RtsAgent agent)
    {
        if (agentUnits.ContainsKey(agent))
        {
            return agentUnits[agent].Contains(unit);
        }

        return false;
    }
    private Reclaim SpawnGaussianReclaim(Vector3 position)  
    {
        Reclaim naturalReclaim = Instantiate(reclaimPrefab, position, Quaternion.identity, transform);
        naturalReclaim.SetRandomGaussianAmount(reclaimMinMax);
        naturalReclaim.onReclaimDestroyed += OnReclaimDestroyed;
        return naturalReclaim;
    }
    
    private void SpawnUnit(RtsAgent owner, Vector3 position, UnitTemplate unitTemplate)
    {
        Unit unit = Instantiate(unitPrefab, position, Quaternion.identity, transform);
        unit.SetUnitTemplate(unitTemplate);
        agentUnits[owner].Add(unit);
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
                Destroy(unit);
            }
            
            agentUnit.Value.Clear();
        }
        
        foreach (RtsAgent rtsAgent in agents)
        {
            for (int i = 0; i < numCommanders; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(-halfGroundSize, halfGroundSize), 
                    0, 
                    Random.Range(-halfGroundSize, halfGroundSize));
            
                SpawnUnit(rtsAgent, position, unitIdTable[commanderTemplateId]);
            }
        }
    }
    private void ResetReclaim()
    {
        foreach (Reclaim r in reclaims)
        {
            Destroy(r);
        }
        
        reclaims.Clear();
        
        for (int i = 0; i < numGaussianReclaim; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize), 
                0, 
                Random.Range(-halfGroundSize, halfGroundSize));
            
            reclaims.Add(SpawnGaussianReclaim(position));
        }
    }

    private void OnDestroy()
    {
        onReclaimDestroyed -= OnReclaimDestroyed;
    }

    public void OnReclaimDestroyed(Reclaim reclaim)
    {
        if (reclaims.Contains(reclaim))
        {
            reclaim.onReclaimDestroyed -= OnReclaimDestroyed;
            reclaims.Remove(reclaim);
        }
        else
        {
            Debug.Log("Received event for destruction of foreign recalim");
        }
    }
}