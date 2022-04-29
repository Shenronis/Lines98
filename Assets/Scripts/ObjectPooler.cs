using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{    
    public static ObjectPooler Instance {get; private set;}

    [Serializable]
    public class Pool {
        public int poolSize;
        public string tag;
        public GameObject prefab;
    }

    public List<Pool> pools;
    
    public Dictionary<string, List<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, List<GameObject>>();
        foreach(Pool pool in pools)
        {
            List<GameObject> objectPool = new List<GameObject>();
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);                
                obj.SetActive(false);
                objectPool.Add(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Get GameObject from Pool, and see if it should spawn immediately
    /// </summary>
    /// <param name="tag">Name tag</param>
    /// <param name="shouldSpawn">Should spawn immediately</param>
    /// <returns>GameObject</returns>
    public GameObject GetFromPool(string tag, bool shouldSpawn=false)
    {
         if (poolDictionary.ContainsKey(tag))
        {
            GameObject spawnedObj = null;
            foreach(var obj in poolDictionary[tag])
            {
                if (!obj.activeInHierarchy) {
                    spawnedObj = obj;
                    break;
                }
            }
            
            if (shouldSpawn) spawnedObj.SetActive(true);
            
            return spawnedObj;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
        }
    }
}
