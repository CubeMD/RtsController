using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Reclaim : MonoBehaviour
{
    public event Action<Reclaim> OnReclaimDestroyed;
    
    [SerializeField] private Renderer ren;
    [SerializeField] private Vector2 reclaimMinMax;
    [SerializeField] private Gradient gradient;
    [SerializeField] private float amount = 1;
    
    public float Amount
    {
        get => amount;
        set
        {
            if (value <= 0)
            {
                SelfDestroy();
                return;
            }
            
            amount = value;
            ren.material.color = gradient.Evaluate(Mathf.InverseLerp(reclaimMinMax.x, reclaimMinMax.y, amount));
        }
    }
    
    private void OnDestroy()
    {
        OnReclaimDestroyed?.Invoke(this);
    }
    
    public void SetRandomGaussianAmount(Vector2 rMinMax)
    {
        SetReclaimMinMax(rMinMax);
        
        float u, s;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            float v = 2.0f * Random.value - 1.0f;
            s = u * u + v * v;
        }
        while (s >= 1.0f);
 
        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
 
        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (rMinMax.x + rMinMax.y) / 2.0f;
        float sigma = (rMinMax.y - mean) / 3.0f;
        Amount = Mathf.Clamp(std * sigma + mean, rMinMax.x, rMinMax.y);
    }
    
    private void SetReclaimMinMax(Vector2 rMinMax)
    {
        reclaimMinMax = rMinMax;
    }

    private void SelfDestroy()
    {
        Destroy(gameObject);
    }
}