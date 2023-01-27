using System.Collections.Generic;
using Interfaces;
using Players;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class MatchManager : MonoBehaviour
{
    public List<Player> players;
    public LayerMask spaceLayerMask;

    [HideInInspector] public List<Reclaim> allReclaim;
    [HideInInspector] public float halfGroundSize;

    [SerializeField] private Transform ground;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numStartingReclaim;
    [SerializeField] private float matchDurationSeconds;
    public Timer matchTimer;

    private void Awake()
    {
        halfGroundSize = ground.localScale.x / 2;
        matchTimer = new Timer(this, matchDurationSeconds, MatchTimeout);
        ResetMap();
        SpawnPlayersCommanders();
    }
    
    private void ResetMap()
    {
        DestroyAllReclaim();
        SpawnStartingReclaim();
        GeneratePlayerStartingPositions();
        matchTimer.StartTimer();
    }

    private void DestroyAllReclaim()
    {
        int numReclaim = allReclaim.Count;
        
        for (int i = 0; i < numReclaim; i++)
        {
            allReclaim[0].DestroyReclaim();
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
        Reclaim reclaim = ObjectPooler.InstantiateGameObject(reclaimPrefab, position, Quaternion.identity, ground.parent);
        reclaim.OnDestroyableDestroy += HandleReclaimDestroyed;
        reclaim.Amount = amount;
            
        allReclaim.Add(reclaim);
    }
    
    public void GeneratePlayerStartingPositions()
    {
        foreach (Player player in players)
        {
            player.startingPosition = new Vector3(
                Random.Range(-halfGroundSize, halfGroundSize),
                0,
                Random.Range(-halfGroundSize, halfGroundSize));
        }
    }
    
    public void HandleReclaimDestroyed(IDestroyable destroyable)
    {
        Reclaim reclaim = destroyable.GetGameObject().GetComponent<Reclaim>();
        reclaim.OnDestroyableDestroy -= HandleReclaimDestroyed;
        allReclaim.Remove(reclaim);
    }

    public void MatchTimeout()
    {
        ResetMatch();
    }
    
    public void ResetMatch()
    {
        ResetPlayers();
        ResetMap();
        SpawnPlayersCommanders();
    }
    
    public void ResetPlayers()
    {
        foreach (Player player in players)
        {
            player.ResetPlayer();
        }
    }

    public void SpawnPlayersCommanders()
    {
        foreach (Player player in players)
        {
            player.unitManager.SpawnCommander();
        }
    }
}