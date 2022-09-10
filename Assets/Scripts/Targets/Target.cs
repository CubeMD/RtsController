using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Targets
{
    public class Target : MonoBehaviour
    {
        [SerializeField] protected Collider col;
        [SerializeField] protected Renderer ren;
        [SerializeField] protected float reward;
    
        protected bool collected;
        private float halfSpawnableSize;
    
        public void Initialize(float halfGroundSize)
        {
            halfSpawnableSize = halfGroundSize;
        }

        public virtual void Reset()
        {
            Vector3 position = new Vector3(Random.Range(-halfSpawnableSize, halfSpawnableSize), 0, Random.Range(-halfSpawnableSize, halfSpawnableSize));
            transform.localPosition = position;
        
            collected = false;
            col.enabled = true;
            ren.enabled = true;
        }

        public virtual void Collect(Agent agent)
        {
            collected = true;
            col.enabled = false;
            ren.enabled = false;
            agent.AddReward(reward);
        }
    
        public float GetReward()
        {
            return reward;
        }

        public bool IsClicked()
        {
            return collected;
        }
    }
}
