using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.ObjectPooling
{
    /// <summary>
    /// Holds unique ID, max amount of instantiations per frame and the queue of object to spawn
    /// </summary>
    public class PoolQueueGroup
    {
        private readonly int maxInstantiationsPerFrame;

        private readonly struct QueuedInstantiation
        {
            public readonly Object prefab;
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Transform parent;

            public QueuedInstantiation(Object prefab, Vector3 position, Quaternion rotation, Transform parent)
            {
                this.prefab = prefab;
                this.position = position;
                this.rotation = rotation;
                this.parent = parent;
            }
        }

        private readonly Queue<QueuedInstantiation> queuedInstantiations;
        private readonly System.Action<Object> callbackFunction;
        private readonly ObjectPooler objectPooler;
        private Coroutine routine;

        public PoolQueueGroup(ObjectPooler objectPooler, int maxInstantiationsPerFrame, System.Action<Object> callbackFunction)
        {
            this.objectPooler = objectPooler;
            this.maxInstantiationsPerFrame = maxInstantiationsPerFrame;
            this.callbackFunction = callbackFunction;
            queuedInstantiations  = new Queue<QueuedInstantiation>();
        }
        
        /// <summary>
        /// Creates and saves the Queued Instantiation that will be added to the end of the queue for instantiation 
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="position">World position of the instantiated object</param>
        /// <param name="rotation">World rotation of the instantiated object</param>
        /// <param name="parent">Transform the instantiated object will be a child of</param>
        public void CreateNewQueuedInstantiation(Object prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            queuedInstantiations.Enqueue(new QueuedInstantiation(prefab, position, rotation, parent));
            routine ??= objectPooler.StartCoroutine(QueueRoutine());
        }
        
        /// <summary>
        /// Instantiates the max allowed amount of the prefabs per frame in the given queue group
        /// Auto terminates when no more Queued Instantiations are in the group
        /// </summary>
        private IEnumerator QueueRoutine()
        {
            // Skips the first frame to allow the queue receive any other queuers to spawn 
            yield return null;

            while (queuedInstantiations.Count > 0)
            {
                // Spawns max allowed amount of the prefabs per frame
                // In case that current amount of prefabs in the queue is less than max allowed
                // Spawns the current amount of prefabs in the queue
                for (int i = 0; i < Mathf.Min(maxInstantiationsPerFrame, queuedInstantiations.Count); i++)
                {
                    QueuedInstantiation queuedInstantiation = queuedInstantiations.Dequeue();
                    Object g = ObjectPooler.Instantiate(queuedInstantiation.prefab, queuedInstantiation.position, queuedInstantiation.rotation, queuedInstantiation.parent);

                    // Invokes the callback function if game object was spawned and the function exists
                    if (g != null && callbackFunction != null)
                    {
                        callbackFunction(g);
                    }
                }
                yield return null;
            }

            routine = null;
        }

        /// <summary>
        /// Clears the queue group by removing all Queued Instantiation and stops the queue routine
        /// </summary>
        public void ClearQueueGroup()
        {
            // Stops the queue coroutine from spawning the prefabs
            if (routine != null && objectPooler != null)
            {
                objectPooler.StopCoroutine(routine);
                routine = null;
            }
            
            queuedInstantiations.Clear();
        }
    }
}