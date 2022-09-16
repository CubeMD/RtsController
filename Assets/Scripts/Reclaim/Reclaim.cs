using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Targets
{
    public abstract class Reclaim : MonoBehaviour
    {
        [SerializeField] protected Renderer ren;
        
        [SerializeField] protected float reward;
        [SerializeField] private Vector2 rewardMinMax;
        [SerializeField] private Gradient gradient;
        public float Reward
        {
            get => reward;
            set
            {
                ren.material.color = gradient.Evaluate(Mathf.InverseLerp(rewardMinMax.x, rewardMinMax.y, reward));
                reward = value;
            }
        }

        public bool Collected { get; private set; }
        public virtual bool IsTerminating => false;

        
        public override void Reset()
        {
            base.Reset();
            RandomGaussian(rewardMinMax.x, rewardMinMax.y);
            Collected = false;
        }

        public virtual void Collect(Agent agent)
        {
            Collected = true;
            gameObject.SetActive(false);
            agent.AddReward(reward);
        }
        
        private void RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
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
            reward = Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
    }
}
