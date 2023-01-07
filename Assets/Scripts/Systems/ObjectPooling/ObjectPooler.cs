using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Systems.ObjectPooling
{
    /// <summary>
    /// ObjectPooler class that efficiently reuses prefab instances to save on Instantiation and Destruction time
    /// </summary>
    public class ObjectPooler : Singleton<ObjectPooler>
    {
        /// <summary>
        /// How many instances of a given prefab can be unloaded (i.e., removed from the pool and destroyed) in 1 frame?
        /// </summary>
        private const int MAX_PER_PREFAB_UNLOADS_PER_FRAME = 1;

        /// <summary>
        /// Keeps track of how many LOD prefab instances of a certain type are active vs. pooled.
        /// </summary>
        private class LODInstanceCounts
        {
            public int activeCount;
            public int pooledCount;

            public LODInstanceCounts(int activeCount, int pooledCount)
            {
                this.activeCount = activeCount;
                this.pooledCount = pooledCount;
            }
        }
        
        [StaticDomainReloadField(instantiateNew: true)]
        private static readonly Dictionary<Object, LODInstanceCounts> lodPrefabInstanceTracking = new Dictionary<Object, LODInstanceCounts>();

        private readonly Dictionary<Object, List<Object>> pooledGameObjects = new Dictionary<Object, List<Object>>();
        private readonly Dictionary<int, Object> prefabByInstanceID = new Dictionary<int, Object>();
        private readonly Dictionary<string, PoolQueueGroup> queueGroups = new Dictionary<string, PoolQueueGroup>();
        private readonly Dictionary<Object, Coroutine> poolUnloadCoroutines = new Dictionary<Object, Coroutine>();

        private void OnDestroy()
        {
            ClearQueueCoroutines();
            ClearPool();
        }
        
        private static void InitializeGameObject<T>(T obj) where T : Object
        {
            GameObject gameObj = obj is Component component ? component.gameObject : (obj as GameObject);
            Object prefabObj = ((T)Instance.prefabByInstanceID[obj.GetInstanceID()]);
            GameObject prefabGameObj =
                prefabObj is Component prefabComp ? prefabComp.gameObject : (prefabObj as GameObject);
            gameObj.transform.localScale = prefabGameObj.transform.localScale;
            gameObj.gameObject.SetActive(true);
        }

        private static T InstantiatePrefab<T>(T prefab, System.Func<T> instantiationFunction) where T : Object
        {
            if (prefab == null || instantiationFunction == null)
            {
                Debug.LogError("Attempting to instantiate a null prefab!");
                return null;
            }
            if (Instance.pooledGameObjects.ContainsKey(prefab))
            {
                List<Object> pooledPrefabInstances = Instance.pooledGameObjects[prefab];

                while (pooledPrefabInstances.Count > 0 && pooledPrefabInstances[0] == null)
                {
                    pooledPrefabInstances.RemoveAt(0);
                }

                if (pooledPrefabInstances.Count > 0)
                {
                    T pooledPrefab = pooledPrefabInstances[0] as T;
                    pooledPrefabInstances.RemoveAt(0);
                    return pooledPrefab;
                }
                else
                {
                    return InstantiateNewPrefab(prefab, instantiationFunction);
                }
            }
            else
            {
                return InstantiateNewPrefab(prefab, instantiationFunction);
            }
        }

        private static T InstantiateNewPrefab<T>(T prefab, System.Func<T> instantiationFunction) where T : Object
        {
            GameObject gameObj = prefab is Component component ? component.gameObject : (prefab as GameObject);

            if (instantiationFunction == null)
            {
                return null;
            }

            T g = instantiationFunction();

            if (g != null)
            {
                Instance.prefabByInstanceID.Add(g.GetInstanceID(), prefab);
            }

            return g;
        }

        /// <summary>
        /// Instantiates a prefab.  If possible, utilizes a previously pooled object
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <returns>Returns the instantiated prefab</returns>
        public new static T Instantiate<T>(T prefab) where T : Object
        {
            return Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Instantiates a prefab.  If possible, utilizes a previously pooled object
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="parent">Transform the instantiated object will be a child of</param>
        /// <param name="isLODObject">If true, this is a LOD object and its active instances will be tracked (for unloading upon complete pooling).</param>
        /// <returns>Returns the instantiated prefab</returns>
        public new static T Instantiate<T>(T prefab, Transform parent) where T : Object
        {
            GameObject gameObj = prefab is Component component ? component.gameObject : (prefab as GameObject);

            bool instantiatingPrefab = gameObj.scene.rootCount == 0;
            T g = InstantiatePrefab(prefab, () =>
            {
#if UNITY_EDITOR
                if (instantiatingPrefab)
                {
                    return UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent) as T;
                }
#endif
                return Object.Instantiate(prefab, parent) as T;
            });
            
            GameObject instanceObject = g is Component instanceComp ? instanceComp.gameObject : (g as GameObject);

            if (instanceObject != null && instanceObject.transform.parent != parent)
            {
                instanceObject.transform.SetParent(parent);
            }

            InitializeGameObject(g);
            return g;
        }

        /// <summary>
        /// Instantiates a prefab.  If possible, utilizes a previously pooled object
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="position">World position of the instantiated object</param>
        /// <param name="rotation">World rotation of the instantiated object</param>
        /// <returns>Returns the instantiated prefab</returns>
        public new static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object
        {
            return Instantiate(prefab, position, rotation, null);
        }
        
        public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, 
            Transform parent) where T : Object
        {
            GameObject prefabGameObj = prefab is Component component ? component.gameObject : (prefab as GameObject);

            bool instantiatingPrefab = prefabGameObj.scene.rootCount == 0;
            T g = InstantiatePrefab(prefab, () =>
            {
#if UNITY_EDITOR
                if (instantiatingPrefab)
                {
                    return UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent) as T;
                }
#endif
                return Object.Instantiate(prefab, position, rotation, parent) as T;
            });

            GameObject instanceGameObj = g is Component comp ? comp.gameObject : (g as GameObject);

            if (instanceGameObj != null)
            {
                if (instanceGameObj.transform.parent != parent)
                {
                    instanceGameObj.transform.SetParent(parent);
                }

                instanceGameObj.transform.position = position;
                instanceGameObj.transform.rotation = rotation;
            }
            
            InitializeGameObject(g);
            return g;
        }
        
        /// <summary>
        /// Pools a game object
        /// </summary>
        /// <param name="obj">GameObject to pool</param>
        public static void PoolGameObject<T>(T obj) where T : Object
        {
            if (Instance == null)
            {
                return;
            }
            
            GameObject gameObj = obj is Component component ? component.gameObject : (obj as GameObject);
            int objectInstanceID = obj.GetInstanceID();

            if (!Instance.prefabByInstanceID.ContainsKey(objectInstanceID))
            {
                Destroy(gameObj);
                return;
            }

            T originalPrefab = Instance.prefabByInstanceID[objectInstanceID] as T;

            if (!Instance.pooledGameObjects.ContainsKey(originalPrefab))
            {
                Instance.pooledGameObjects.Add(originalPrefab, new List<Object>());
            }

            if (!Instance.pooledGameObjects[originalPrefab].Contains(obj))
            {
                Instance.pooledGameObjects[originalPrefab].Add(obj);
            }

            if (gameObj == null || !gameObj.activeSelf)
            {
                return;
            }
            
            gameObj.SetActive(false);
        }

        private IEnumerator PoolAfterParticlesVanish(GameObject obj, bool reset, ParticleSystem[] attachedParticles)
        {
            foreach (ParticleSystem particle in attachedParticles)
            {
                if (particle.main.cullingMode != ParticleSystemCullingMode.AlwaysSimulate)
                {
                    Debug.LogWarning("Particle system does not use AlwaysSimulate culling mode. " +
                                     "Pooling behaviour may not work as expected", particle);
                }
                
                particle.Stop();
            }

            bool particlesDisappeared = false;
            float trackedTime = 0f;
                
            while (!particlesDisappeared && trackedTime < 5f)
            {
                particlesDisappeared = true;
                    
                foreach (ParticleSystem particle in attachedParticles)
                {
                    particlesDisappeared &= particle.particleCount > 0;
                }
                
                yield return null;
                trackedTime += Time.deltaTime;
            }
            
            PoolGameObject(obj);
        }

        /// <summary>
        /// Destroys all pooled GameObjects
        /// </summary>
        public static void ClearPool()
        {
            if(Instance == null)
                return;
            
            foreach (Coroutine coroutine in Instance.poolUnloadCoroutines.Values)
            {
                if (coroutine != null)
                {
                    Instance.StopCoroutine(coroutine);
                }
            }
            
            foreach (List<Object> spawnedObjectsOfPrefab in Instance.pooledGameObjects.Values)
            {
                if (spawnedObjectsOfPrefab == null)
                {
                    continue;
                }

                foreach (Object spawnedObject in spawnedObjectsOfPrefab)
                {
                    if (spawnedObject != null)
                    {
                        Instance.prefabByInstanceID.Remove(spawnedObject.GetInstanceID());
                        Destroy(spawnedObject is Component component? component.gameObject : (spawnedObject as GameObject));
                    }
                }

                spawnedObjectsOfPrefab.Clear();
            }
        }

        /// <summary>
        /// Destroys all pooled GameObjects tied to a given prefab, and removes the prefab-key entry
        /// from the dictionary of pooled objects.
        /// </summary>
        private void StartUnloadingGameObjectsOfTypeFromPool<T>(T prefab) where T : Object
        {
            if (!poolUnloadCoroutines.ContainsKey(prefab))
            {
                poolUnloadCoroutines[prefab] = StartCoroutine(PrefabInstancesPoolUnload(prefab));
            }
        }

        private IEnumerator PrefabInstancesPoolUnload<T>(T prefab) where T : Object
        {
            List<Object> instances = pooledGameObjects[prefab];
            
            while (instances.Count > 0)
            {
                int numInstances = instances.Count;
                int toUnloadThisFrame = Mathf.Min(numInstances, MAX_PER_PREFAB_UNLOADS_PER_FRAME);

                for (int i = 0; i < toUnloadThisFrame; i++)
                {
                    int endOfList = numInstances - 1 - i; // Remove instances from end of list
                    
                    T instance = instances[endOfList] as T;
                    instances.RemoveAt(endOfList);

                    if (instance != null)
                    {
                        Instance.prefabByInstanceID.Remove(instance.GetInstanceID());
                        Destroy(instance is Component component? component.gameObject : (instance as GameObject));
                    }
                }

                yield return null;
            }

            // Remove the coroutine from the coroutines dictionary.
            poolUnloadCoroutines.Remove(prefab);
            
            // Note that even if the pooledGameObjects list is empty, it is never removed from
            // its containing dictionary once it has been added.
        }

        /// <summary>
        /// Instantiates a set number of prefabs directly into the object pool over several frames
        /// </summary>
        public static void PreloadObjects(Object prefab, int count, Transform parent = null)
        {
            Instance.StartCoroutine(Instance.PreloadObjectsRoutine(prefab, count, parent));
        }
        
        /// <summary>
        /// Instantiates a set number of prefabs directly into the object pool over several frames
        /// </summary>
        public static void PreloadObjects<T>(List<T> prefabs, int count, Transform parent = null) where T : Object
        {
            foreach (T prefab in prefabs)
            {
                PreloadObjects(prefab, count, parent);
            }
        }
    
        private IEnumerator PreloadObjectsRoutine(Object prefab, int count, Transform parent)
        {
            while (!Instance.pooledGameObjects.ContainsKey(prefab) ||
                   Instance.pooledGameObjects[prefab].Count < count)
            {
                PoolGameObject(InstantiateNewPrefab(prefab,
                    () => GameObject.Instantiate(prefab, Vector3.one * 150000, Quaternion.identity, parent)));
                yield return null;
            }
        }
        
        #region Queue

        /// <summary>
        /// Creates a queue group with the ID to limit the amount of instantiation per frame for this specific group
        /// Instantiates a set number of prefabs directly into the object pool over several frames
        /// </summary>
        /// <param name="queueID">The queue id associated with one queue group</param>
        /// <param name="maxQueueSpawnPerFrame">The max amount of object allowed to be instantiated per frame</param>
        /// <param name="callbackFunction">The callback method that will be invoked on instantiation of the game object</param>
        /// <param name="prefab">The prefab to preload</param>
        /// <param name="preloadCount">Amount of the each prefab to preload</param>
        public static void CreateQueueAndPreload<T>(string queueID, int maxQueueSpawnPerFrame, System.Action<Object> callbackFunction, T prefab, int preloadCount = 0) where T : Object
        {
            CreateQueue(queueID, maxQueueSpawnPerFrame, callbackFunction);
            
            if(preloadCount > 0)
                PreloadObjects(prefab, preloadCount);
        }

        /// <summary>
        /// Creates a queue group with the ID to limit the amount of instantiation per frame for this specific group
        /// Instantiates a set number of prefabs of each type directly into the object pool over several frames
        /// </summary>
        /// <param name="queueID">The queue id associated with one queue group</param>
        /// <param name="maxQueueSpawnPerFrame">The max amount of object allowed to be instantiated per frame</param>
        /// <param name="callbackFunction">The callback method that will be invoked on instantiation of the game object</param>
        /// <param name="prefabs">List of prefabs to preload</param>
        /// <param name="preloadCount">Amount of the each prefab to preload</param>
        public static void CreateQueueAndPreload<T>(string queueID, int maxQueueSpawnPerFrame, System.Action<Object> callbackFunction, List<T> prefabs, int preloadCount = 0) where T : Object
        {
            CreateQueue(queueID, maxQueueSpawnPerFrame, callbackFunction);

            if (preloadCount > 0)
            {
                foreach (T prefab in prefabs)
                {
                    PreloadObjects(prefab, preloadCount);
                }
            }
        }

        /// <summary>
        /// Creates a queue group with the ID to limit the amount of instantiation per frame for this specific group
        /// </summary>
        /// <param name="queueID">The queue group id</param>
        /// <param name="maxQueueSpawnPerFrame">The max amount of object allowed to be instantiated per frame</param>
        /// <param name="callbackFunction">The callback method that will be invoked on instantiation of the game object</param>
        public static void CreateQueue(string queueID, int maxQueueSpawnPerFrame, System.Action<Object> callbackFunction)
        {
            if (Instance.queueGroups.ContainsKey(queueID))
            {
                Debug.LogError("Attempting to create the queue that has already been created!");
                return;
            }
            
            // Creates a new queue group with the passed parameters
            Instance.queueGroups.Add(queueID, new PoolQueueGroup(Instance, maxQueueSpawnPerFrame, callbackFunction));
        }
        
        /// <summary>
        /// Instantiates a prefab in the queue based on max allowed spawns per frame. If possible, utilizes a previously pooled object
        /// And returns the instantiated game object to the callback function associated with this queue
        /// </summary>
        /// <param name="queueID">The id of the queue group to which the prefab will be added</param>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="position">World position of the instantiated object</param>
        /// <param name="rotation">World rotation of the instantiated object</param>
        /// <param name="parent">Transform the instantiated object will be a child of</param>
        public static void InstantiateInQueue<T>(string queueID, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            if (Instance.queueGroups.ContainsKey(queueID))
            {
                PoolQueueGroup poolQueueGroup = Instance.queueGroups[queueID];
                poolQueueGroup.CreateNewQueuedInstantiation(prefab, position, rotation, parent);
            }
            else
            {
                Debug.LogWarning($"No queue exist for this queue ID : {queueID}. Please call CreateQueue to create a queue");
            }
        }

        /// <summary>
        /// Stops the queue group instantiation and removes all its queued instantiations
        /// </summary>
        /// <param name="queueID">The queue group id</param>
        public static void StopQueueInstantiationByID(string queueID)
        {
            if (Instance.queueGroups.ContainsKey(queueID))
            {
                // Gets the queue group with this ID
                PoolQueueGroup poolQueueGroup = Instance.queueGroups[queueID];
                
                // Clears the queue group and its Queued Instantiations
                // If there is queue routine currently running
                // Stops this coroutine 
                poolQueueGroup.ClearQueueGroup();
            }
            else
            {
                Debug.LogWarning($"No queue exist for this queue ID : {queueID}. Please call CreateQueue to create a queue");
            }
        }
        
        /// <summary>
        /// Clears all queue groups by calling internal clear method
        /// </summary>
        private void ClearQueueCoroutines()
        {
            // Clears the queue groups and its queuers
            foreach (PoolQueueGroup queueGroup in queueGroups.Values)
            {
                queueGroup.ClearQueueGroup();
            }
        }
        
        #endregion
    }
}