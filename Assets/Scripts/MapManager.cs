using System.Collections.Generic;
using Interfaces;
using Players;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    public List<Reclaim> allReclaim;
    public Transform ground;
    public LayerMask spaceLayerMask;
    
    public readonly Dictionary<Player, Vector3> playerStartingPositions = new Dictionary<Player, Vector3>();
    
    [HideInInspector] public float halfGroundSize;

    [SerializeField] private Environment environment;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numStartingReclaim;
        
    private void Awake()
    {
        halfGroundSize = ground.localScale.x / 2;
        HandleEnvironmentReset();
    }

    public void HandleEnvironmentReset()
    {
        DestroyAllReclaim();
        SpawnStartingReclaim();
        GeneratePlayerStartingPositions(environment.players);
        
        foreach (Player player in environment.players)
        {
            player.unitManager.SpawnCommander();
        }
    }

    public void GeneratePlayerStartingPositions(List<Player> players)
    {
        playerStartingPositions.Clear();
    
        foreach (Player player in players)
        {
            Vector3 position = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize),
                0,
                Random.Range(-halfGroundSize, halfGroundSize));
            
            playerStartingPositions.Add(player, position);
        }
    }

    public void SpawnStartingReclaim()
    {
        for (int i = 0; i < numStartingReclaim; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize),
                0,
                Random.Range(-halfGroundSize, halfGroundSize));
                
            SpawnReclaim(position, 100);
        }
    }

    public void SpawnReclaim(Vector3 position, float amount)
    {
        Reclaim reclaim = ObjectPooler.InstantiateGameObject(reclaimPrefab, position, Quaternion.identity, transform);
        reclaim.OnDestroyableDestroy += HandleReclaimDestroyed;
        reclaim.Amount = amount;
            
        allReclaim.Add(reclaim);
    }

    public void HandleReclaimDestroyed(IDestroyable destroyable)
    {
        Reclaim reclaim = destroyable.GetGameObject().GetComponent<Reclaim>();
        reclaim.OnDestroyableDestroy -= HandleReclaimDestroyed;
        allReclaim.Remove(reclaim);
    }

    public void DestroyAllReclaim()
    {
        int numReclaim = allReclaim.Count;
        
        for (int i = 0; i < numReclaim; i++)
        {
            allReclaim[allReclaim.Count - 1].DestroyReclaim();
        }
    }
}