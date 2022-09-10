using UnityEngine;
using Random = UnityEngine.Random;

namespace Targets
{
    public class GaussianTarget : Target
    {
        [SerializeField] private Vector2 rewardMinMax;
        [SerializeField] private Gradient gradient;
    
        public override void Reset()
        {
            base.Reset();
        
            reward = RandomGaussian(rewardMinMax.x, rewardMinMax.y);
            ren.material.color = gradient.Evaluate(Mathf.InverseLerp(rewardMinMax.x, rewardMinMax.y, reward));
        }
    
        private static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
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
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
    }
}
