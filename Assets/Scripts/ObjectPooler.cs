using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Singleton
    public static ObjectPooler Instance {get; private set;}
    void Awake()
    {
        Instance = this;
    }
    #endregion
    
    [System.Serializable]
    public class Pool {
        public int poolSize;
        public string tag;
        public GameObject prefab;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.name = pool.prefab.GetComponent<Ball>().type.ToString();
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            GameObject spawnedObj = poolDictionary[tag].Dequeue();
            spawnedObj.SetActive(true);

            poolDictionary[tag].Enqueue(spawnedObj);

            return spawnedObj;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
        }
    }    
}
