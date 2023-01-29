using System.Collections.Generic;
using Interfaces;
using Players;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private List<Player> players;
    [SerializeField] private LayerMask spaceLayerMask;
    public LayerMask SpaceLayerMask => spaceLayerMask;

    [SerializeField] private Transform ground;
    [SerializeField] private Reclaim reclaimPrefab;
    [SerializeField] private int numStartingReclaim;
    [SerializeField] private float matchDurationSeconds;
    
    public float HalfGroundSize { get; private set; }
    public float TimerPercentage => matchTimer.TimerPercentage;
    
    private Timer matchTimer;
    private readonly List<Reclaim> allReclaim = new List<Reclaim>();

    private void Awake()
    {
        HalfGroundSize = ground.localScale.x / 2;
        matchTimer = new Timer(this, matchDurationSeconds, MatchTimeout);
        ResetMap();
        SpawnPlayersCommanders();
    }

    private void SpawnStartingReclaim()
    {
        for (int i = 0; i < numStartingReclaim; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-HalfGroundSize, HalfGroundSize),
                0,
                Random.Range(-HalfGroundSize, HalfGroundSize));
                
            SpawnReclaim(position, 100);
        }
    }

    private void SpawnReclaim(Vector3 position, float amount)
    {
        Reclaim reclaim = ObjectPooler.InstantiateGameObject(reclaimPrefab, position, Quaternion.identity, ground.parent);
        reclaim.OnDestroyableDestroy += HandleReclaimDestroyed;
        reclaim.Amount = amount;
            
        allReclaim.Add(reclaim);
    }

    private void GeneratePlayerStartingPositions()
    {
        foreach (Player player in players)
        {
            player.startingPosition = new Vector3(
                Random.Range(-HalfGroundSize, HalfGroundSize),
                0,
                Random.Range(-HalfGroundSize, HalfGroundSize));
        }
    }

    public bool TryGetClosesReclaimToPosition(Vector3 position, out Reclaim closestReclaim)
    {
        closestReclaim = null;
        float minFoundDistance = float.MaxValue;
        float currentDistance;

        foreach (Reclaim reclaim in allReclaim)
        {
            currentDistance = Vector3.Distance(reclaim.transform.position, position);

            if (currentDistance < minFoundDistance)
            {
                minFoundDistance = currentDistance;
                closestReclaim = reclaim;
            }
        }

        return closestReclaim != null;
    }
    
    private void HandleReclaimDestroyed(IDestroyable destroyable)
    {
        Reclaim reclaim = destroyable.GetGameObject().GetComponent<Reclaim>();
        reclaim.OnDestroyableDestroy -= HandleReclaimDestroyed;
        allReclaim.Remove(reclaim);
    }

    private void MatchTimeout()
    {
        ResetMatch();
    }

    private void ResetMatch()
    {
        ResetPlayers();
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

    private void ResetPlayers()
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