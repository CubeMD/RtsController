using System;
using System.Collections.Generic;
using Players;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public event Action OnEnvironmentReset;
    public MapManager mapManager;
    public List<Player> players;
    public float timeWhenReset;
    
    [HideInInspector] public float timeSinceReset;
    
    private void Awake()
    {
        //Academy.Instance.AutomaticSteppingEnabled = false;
        
        OnEnvironmentReset += mapManager.HandleEnvironmentReset;
        
        foreach (Player player in players)
        {
            OnEnvironmentReset += player.HandleEnvironmentReset;
        }
    }
    
    private void OnDestroy()
    {
        OnEnvironmentReset -= mapManager.HandleEnvironmentReset;
        
        foreach (Player player in players)
        {
            OnEnvironmentReset -= player.HandleEnvironmentReset;
        }
    }

    public void FixedUpdate()
    {
        timeSinceReset += Time.fixedDeltaTime;
        
        if (timeSinceReset >= timeWhenReset)
        {
            ResetEnvironment();
        }
    }
    
    public void ResetEnvironment()
    {
        OnEnvironmentReset?.Invoke();
        timeSinceReset = 0;
        Resources.UnloadUnusedAssets();
    }
}