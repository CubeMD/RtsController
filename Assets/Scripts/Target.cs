using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    [SerializeField] private Collider col;
    [SerializeField] private Renderer ren;
    [SerializeField] private Vector2 rewardMinMax;
    [SerializeField] private Gradient gradient;

    private float reward;
    private bool collected;
    private float halfSpawnableSize;
    
    public void Initialize(float halfGroundSize)
    {
        halfSpawnableSize = halfGroundSize;
        Reset();
    }

    public void Reset()
    {
        Vector3 position = new Vector3(Random.Range(-halfSpawnableSize, halfSpawnableSize), 0, Random.Range(-halfSpawnableSize, halfSpawnableSize));
        transform.localPosition = position;
        
        reward = RandomGaussian(rewardMinMax.x, rewardMinMax.y);
        ren.material.color = gradient.Evaluate(Mathf.InverseLerp(rewardMinMax.x, rewardMinMax.y, reward));
        
        collected = false;
        col.enabled = true;
        ren.enabled = true;
    }

    public float Collect()
    {
        collected = true;
        col.enabled = false;
        ren.enabled = false;
        return reward;
    }
    
    public float GetReward()
    {
        return reward;
    }

    public bool IsClicked()
    {
        return collected;
    }
    
    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;
 
        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);
 
        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
 
        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
